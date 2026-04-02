# Properties

owner: Codex

status: draft

stop_state_at: 2026-04-02

title: 도로명 검색 기능 Detailed Design

parent_docs:
- ../plan.md
- ../../../../road-name-search-usecase-eventstorming.md

# Detailed Design

이 문서는 2026-04-02 partial stop-state 기준 상세 설계 기록이다. `python3 .agents/skills/docs-verify/scripts/run.py`는 실행되어 통과했고, `dotnet build TrafficSolution.slnx`와 `dotnet test TrafficSolution.slnx`는 `NETSDK1100`으로 실패했다.

## Design Decisions

- 검색 데이터 소스는 `OpenStreet planet_osm_line`이 아니라 `vds + vds_loc`를 우선 사용한다.
- 클릭용 `IOpenStreetQueryPort`는 유지하고, 도로명 검색 책임은 `IRoadNameHighwaySearchPort`로 분리한다.
- 형태소 확장 지점은 `IRoadNameQueryExpanderPort`로 두고 기본 구현은 no-op normalize에 머문다.
- 클릭과 검색의 공통 후속 경로는 `후보 고속도로가 정해진 뒤`부터 공유한다.
- 혼잡도 모드는 매칭된 고속도로 집합 전체를 사용한다.
- CCTV 모드는 최상위 매칭 1건만 선택한다.

## Matching Rules

- 기본 normalize: trim, 연속 공백 축소, 대소문자 무시
- exact: 정규화된 검색어와 정규화된 도로명이 동일
- partial: 정규화된 도로명이 검색어를 포함하거나 검색어가 도로명 핵심 문자열을 포함
- exact 결과가 1건 이상이면 partial은 버린다
- typo correction, 발음 유사도, 외부 형태소 엔진 연동은 이번 범위에 넣지 않는다

## Interface Signatures

### Search input and result contracts

```csharp
public class SearchRoadByNameCommand
{
    public string Query { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double MinLongitude { get; set; }
    public double MinLatitude { get; set; }
    public double MaxLongitude { get; set; }
    public double MaxLatitude { get; set; }

    public void NormalizeBounds();
}

public enum RoadNameMatchKind
{
    None,
    Exact,
    Partial
}

public class RoadNameSearchResult
{
    public IReadOnlyList<HighWay> Highways { get; init; } = Array.Empty<HighWay>();
    public RoadNameMatchKind MatchKind { get; init; }
}
```

### New ports

```csharp
public interface IRoadNameQueryExpanderPort
{
    IReadOnlyList<string> Expand(string query);
}

public interface IRoadNameHighwaySearchPort
{
    Task<IReadOnlyList<HighWay>> SearchExactAsync(
        IReadOnlyList<string> normalizedQueries,
        double minLongitude,
        double minLatitude,
        double maxLongitude,
        double maxLatitude);

    Task<IReadOnlyList<HighWay>> SearchPartialAsync(
        IReadOnlyList<string> normalizedQueries,
        double minLongitude,
        double minLatitude,
        double maxLongitude,
        double maxLatitude);
}
```

### App services

```csharp
public class SearchRoadByNameService
{
    public SearchRoadByNameService(
        IRoadNameHighwaySearchPort roadNameHighwaySearchPort,
        IRoadNameQueryExpanderPort roadNameQueryExpanderPort);

    public Task<RoadNameSearchResult> SearchAsync(SearchRoadByNameCommand command);
}
```

### Common downstream contracts

```csharp
public class RequestTrafficByPosService
{
    public Task<Dictionary<int, List<VdsTrafficResult>>> GetAdjacentHighWays(
        UpdateSelectedPosTrafficInfoCommand command);

    public Task<Dictionary<int, List<VdsTrafficResult>>> GetTrafficByHighwaysAsync(
        IEnumerable<int> highwayNumbers,
        UpdateSelectedPosTrafficInfoCommand command);
}

public class RequestCctvByPosService
{
    public Task<HighwayCctvSelection> GetNearbyHighwayCctv(
        UpdateSelectedPosCctvInfoCommand command);

    public Task<HighwayCctvSelection> GetHighwayCctvAsync(
        int highwayNo,
        string highwayName,
        UpdateSelectedPosCctvInfoCommand command);
}
```

### UI wrappers

```csharp
private Task RunTrafficLookupAsync(
    UpdateSelectedPosTrafficInfoCommand command,
    IReadOnlyList<int>? selectedHighwayNumbers = null);

private Task RunCctvLookupAsync(
    UpdateSelectedPosCctvInfoCommand command,
    int? selectedHighwayNo = null,
    string? selectedHighwayName = null);
```

- `RunTrafficLookupAsync(...)`
  - 검색/클릭 진입 모두에서 중복 요청을 막고, 상태바를 시작/완료/실패 상태로 갱신한다.
  - 최신 요청의 응답만 화면에 반영하고, 우측 패널의 혼잡도 카드 렌더링을 공통화한다.
  - 검색 결과가 없으면 기존 혼잡도 결과 컨텍스트를 비운다.
- `RunCctvLookupAsync(...)`
  - 검색/클릭 진입 모두에서 중복 요청을 막고, 상태바를 시작/완료/실패 상태로 갱신한다.
  - 최신 요청의 응답만 화면에 반영하고, 우측 패널의 CCTV 카드 렌더링을 공통화한다.
  - 검색 결과가 없으면 기존 CCTV 결과 컨텍스트를 비운다.

## Adapter and Repository Responsibilities

- `DefaultRoadNameQueryExpanderAdapter`
  - 입력 query를 trim하고 연속 공백을 축소한다.
  - 기본 구현에서는 원문 normalize 결과만 반환한다.
- `RoadNameHighwaySearchAdapter`
  - `IRoadNameHighwaySearchPort`를 구현한다.
  - `VdsRepository`를 사용해 bounds 내부의 distinct highway 후보를 exact/partial로 나눠 조회한다.
- `VdsRepository`
  - `vds`와 `vds_loc`를 조합해 `현재 bounds 내부 + 실제 VDS가 있는 도로` 후보만 반환한다.

## UI Contract

- 검색 UI는 `TextBox + Button` 구성으로 동작한다.
- Enter key는 버튼 클릭과 동일한 검색 실행으로 연결한다.
- 검색 시작 전 WebView2에서 `getMapViewState()`를 호출해 center/bounds를 읽는다.
- 검색은 지도 클릭 모드 여부와 무관하게 실행 가능하며, `NearbyHighwayLookup` 선행을 요구하지 않는다.
- 현재 우측 패널 모드가 `혼잡도`면 traffic wrapper, `CCTV`면 CCTV wrapper를 호출한다.
- 검색 결과가 없으면 이전 결과 컨텍스트를 비우고 상태 메시지를 갱신한다.

## Test Points

- `SearchRoadByNameServiceTest`
  - exact가 partial보다 우선되는지 검증
  - bounds 정규화가 검색 포트 호출 전에 적용되는지 검증
  - expander 포트가 호출되는지 검증
  - 빈 검색어/공백 검색어 처리 검증
- `RequestTrafficServiceTest`
  - 공통 downstream 메서드가 전달받은 고속도로 집합과 bounds로 조회하는지 검증
  - 인접 고속도로 조회 결과가 공통 downstream 경로와 동일한 bounds를 사용해 조회되는지 검증
- `RequestCctvByPosServiceTest`
  - 직접 선택된 highway 기준 CCTV downstream 공통 경로 검증
  - 최근접 고속도로 선택 후 이름/거리 기반 CCTV 필터링 계약 검증
- `DiWiringTest`
  - 신규 포트/어댑터 DI 등록 확인

## File Mapping

| Layer | Files |
|---|---|
| Composition Root | `TrafficForm/Program.cs` |
| UI | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs` |
| App | `TrafficForm/App/RequestTrafficByPosService.cs`, `TrafficForm/App/RequestCctvByPosService.cs`, `TrafficForm/App/SearchRoadByNameService.cs`, `TrafficForm/App/SearchRoadByNameCommand.cs` |
| Port | `TrafficForm/Port/IRoadNameHighwaySearchPort.cs`, `TrafficForm/Port/IRoadNameQueryExpanderPort.cs` |
| Adapter | `TrafficForm/Adapter/RoadNameHighwaySearchAdapter.cs`, `TrafficForm/Adapter/DefaultRoadNameQueryExpanderAdapter.cs`, `TrafficForm/Adapter/VdsRepository.cs` |
| Test | `TestProject1/SearchRoadByNameServiceTest.cs`, `TestProject1/RequestTrafficServiceTest.cs`, `TestProject1/RequestCctvByPosServiceTest.cs`, `TestProject1/DiWiringTest.cs` |
| Docs | `docs/road-name-search-usecase-eventstorming.md`, `docs/usecase-spec.md`, `README.md` |
