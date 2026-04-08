# Properties

owner: Codex

status: draft

stop_state_at: 2026-04-02

title: 도로명 검색 기능 Domain Boundary

parent_docs:
- ../plan.md
- ../../../../road-name-search-usecase-eventstorming.md

# Domain Boundary

이 문서는 2026-04-02 partial stop-state 기준 설계 기록이다. `python3 .agents/skills/docs-verify/scripts/run.py`는 실행되어 통과했고, `dotnet build TrafficSolution.slnx`와 `dotnet test TrafficSolution.slnx`는 `NETSDK1100`으로 실패했다.

## 배경

- 사용자는 좌표 클릭 외에 `도로명 검색`으로도 현재 지도 범위 안의 고속도로 조회를 시작할 수 있어야 한다.
- 검색 진입이어도 결과 화면은 기존 클릭 기반 혼잡도/CCTV 후속 동작과 동일해야 한다.
- 검색 매칭 규칙과 query 확장은 구현체 교체가 가능한 포트로 분리해야 한다.

## 현재 상태 입력

- 지도 클릭 진입점은 `TrafficForm/UI/Form1.cs`의 `WebView21_WebMessageReceived`다.
- 혼잡도 downstream은 `UpdateSelectedPosTrafficInfoFromMessage -> RequestTrafficByPosService.GetAdjacentHighWays -> ShowHighwayPanel`이다.
- CCTV downstream은 `UpdateSelectedPosCctvInfoFromMessage -> RequestCctvByPosService.GetNearbyHighwayCctv -> ShowCctvPanel`이다.
- 지도 JS는 검색 시 재사용할 수 있는 `getMapViewState()`를 이미 제공한다.
- `IOpenStreetQueryPort`는 클릭 기반 인접 고속도로 조회만 담당하므로, 도로명 검색 책임은 별도 포트가 자연스럽다.
- `VdsRepository`는 `vds`와 `vds_loc`를 조회하고 있어 `bounds 내부 + 실제 VDS가 있는 도로` 검색 소스로 재사용 가능하다.

## 도메인 경계

- 도메인: `RoadSearch`
- 주 액터: 사용자
- 지원 액터:
  - WebView2 지도 JS
  - `SearchRoadByNameService`
  - `RequestTrafficByPosService`
  - `RequestCctvByPosService`
  - `IRoadNameHighwaySearchPort`
  - `IRoadNameQueryExpanderPort`

## 경계 안에 포함되는 책임

- 현재 지도 center/bounds 수집
- 검색어 normalize 및 확장 포트 호출
- 현재 bounds 내부 도로명 exact/partial 검색
- 현재 우측 패널 모드별 downstream 공통 경로 선택
- 검색 miss 시 현재 결과 컨텍스트 정리와 상태 메시지 갱신

## 경계 밖에 두는 책임

- 형태소 분석기 실제 연동
- fuzzy search, typo correction
- 자동완성 UI
- 전체 지도 범위를 벗어난 전역 검색

## 시스템 경계 매핑

| 영역 | 포함 책임 | 비고 |
|---|---|---|
| UI (`Form1`, `Form1.Cctv`) | 검색 입력, map view state 요청, downstream wrapper 호출 | 현재 모드에 따라 혼잡도/CCTV 흐름 분기 |
| App (`SearchRoadByNameService`) | 입력 검증, bounds 정규화, tier 선택 | exact 우선 정책 보유 |
| Port/Adapter | query 확장, bounds 내부 도로 검색 | 구현 교체 가능 |
| Existing downstream services | 교통/CCTV 조회 및 패널 렌더링 | 클릭과 검색이 공통 사용 |
