# 도로명 검색 기능 설계서 (Use Case + Event Storming)

## 0) 관련 문서

- 유스케이스 식별자: `UC-SCH-001`
- 구현 기준 유스케이스 문서: [`docs/usecase-spec.md`](usecase-spec.md)
- 현재 repo의 계획 문서: [`docs/exec-plans/completed/road-name-search/plan.md`](exec-plans/completed/road-name-search/plan.md)
- raw execute output 기준 계획 경로: `docs/exec-plans/active/road-name-search/plan.md`
- 본 문서는 2026-04-02 execute output과 이후 schema fix를 반영한 설계/구현 추적 문서다.

---

## 1) 설계 배경 / 문제 정의

- WinForms 앱에 `도로명 검색`을 추가한다.
- 검색 대상은 항상 `현재 map bounds 내부`로 제한한다.
- 검색 결과는 `현재 우측 패널 모드(혼잡도/CCTV)`에 맞춰, 기존 지도 클릭 후속 동작과 동일한 결과 화면으로 이어져야 한다.
- 검색 매칭은 `포트/어댑터`로 추상화한다.
- 기본 매칭 우선순위는 `정확히 일치 > like 부분 일치`다.
- 형태소 분리/확장 검색은 지금 구현하지 않더라도, 나중에 쉽게 끼울 수 있는 확장 지점을 만든다.
- 좌표 클릭 처리와 검색 처리의 `후속 조회/렌더링 경로`는 공통화한다.

---

## 2) 구현 결과 요약

- 도로명 검색 유스케이스가 새 App/Port/Adapter 체인으로 추가된 상태다.
- 핵심 진입점은 `TrafficForm/App/SearchRoadByNameService.cs`, `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs`, `TrafficForm/Adapter/VdsRepository.cs`다.
- UI는 `SearchRoadByNameService`를 호출하고, 서비스는 `IRoadNameQueryExpanderPort`와 `IRoadNameHighwaySearchPort`를 거쳐 `VdsRepository` 기반 검색을 수행한다.
- 클릭 기반 혼잡도/CCTV 흐름은 후보 고속도로 결정 뒤부터 공통 downstream wrapper를 재사용하도록 정리된 상태다.
- 외부 HTTP API, DB schema, 환경변수 계약 변경은 없다.
- `python3 .agents/skills/docs-verify/scripts/run.py`는 실행되어 통과했고, `dotnet build TrafficSolution.slnx`와 `dotnet test TrafficSolution.slnx`는 Linux에서 `NETSDK1100`으로 실패했다.
- `TrafficForm/osm-local/compose.yaml` 기준 로컬 Docker Postgres 스키마를 점검한 결과 `public.vds`에는 `도로명`이 없고 `public.vds_loc`에만 있어, `TrafficForm/Adapter/VdsRepository.cs`의 bounds 검색 쿼리는 `vl."도로명"`을 사용하도록 수정된 상태다.

---

## 3) 도메인, 액터/액션

- 도메인: `RoadSearch`
- 액터: 사용자
- 액션
  1. 현재 지도 bounds 안에서 도로명을 입력하고 검색을 실행한다.
  2. 우측 패널 현재 모드(혼잡도/CCTV)에 맞는 조회 흐름으로 진입한다.
  3. 검색 결과가 없으면 현재 결과 컨텍스트가 비워지고 상태 메시지를 확인한다.

---

## 4) 유스케이스

### UC-SCH-001 도로명 검색 기반 조회

- 사전 조건
  - WebView2 지도가 로딩 완료 상태다.
  - 현재 지도 뷰의 center/bounds를 `getMapViewState()`로 읽을 수 있다.
- 성공 시나리오
  1. 사용자가 좌측 검색창에 도로명을 입력하고 실행한다.
  2. 시스템이 현재 지도 center/bounds를 수집해 `SearchRoadByNameCommand`에 담는다.
  3. 시스템이 검색어를 검증하고 bounds를 정규화한다.
  4. 시스템이 검색어 확장/정규화 포트를 호출한 뒤, 현재 bounds 내부에서 실제 VDS가 있는 도로만 exact > partial 순서로 검색한다.
  5. 시스템이 우측 패널 모드를 확인하고, 클릭 이후와 동일한 downstream 공통 wrapper를 호출한다.
  6. 혼잡도 모드면 매칭된 고속도로 집합 전체를 대상으로 기존 클릭 결과와 동일한 카드/마커/세그먼트를 표시한다.
  7. CCTV 모드면 최상위 매칭 1건만 선택해 기존 클릭 결과와 동일한 CCTV 카드/마커를 표시한다.
- 대체 시나리오
  - 빈 검색어/공백 검색어면 조회를 시작하지 않는다.
  - 검색 결과가 없으면 이전 결과를 유지하지 않고 현재 결과 컨텍스트를 비운 뒤 상태 메시지로 안내한다.
- 완료 기준
  - 현재 map bounds 내부 도로만 검색된다.
  - exact match가 있으면 exact 결과만 사용한다.
  - exact가 없을 때만 partial like 결과를 사용한다.
  - 혼잡도/CCTV 결과 화면은 기존 지도 클릭 후속 동작과 동일한 렌더링 경로를 재사용한다.

---

## 5) 이벤트 스토밍 결과

### 5.1 Command

- `SearchRoadByNameCommand`
- `RunTrafficLookupAsync`
- `RunCctvLookupAsync`

### 5.2 Event

- `RoadNameSearchRequested`
- `RoadNameSearchMatched`
- `RoadNameSearchMissed`
- `TrafficLookupCompleted`
- `CctvLookupCompleted`
- `CurrentLookupContextCleared`

### 5.3 Policy

- 검색 대상은 항상 현재 map bounds 내부로 제한한다.
- 검색 데이터 소스는 `OpenStreet planet_osm_line`이 아니라 `vds + vds_loc`를 우선 사용한다.
- exact 결과가 1건 이상이면 partial은 버리고 exact만 반환한다.
- 형태소 확장 지점은 `IRoadNameQueryExpanderPort` 교체만으로 확장 가능해야 한다.
- 클릭과 검색은 `후보 고속도로가 정해진 뒤`부터 공통 downstream 경로를 사용한다.
- 혼잡도 모드는 `매칭된 고속도로 집합 전체`를 대상으로 결과를 그린다.
- CCTV 모드는 `최상위 매칭 1건`만 선택해 기존 단일 고속도로 CCTV 흐름으로 보낸다.
- 검색 결과가 없으면 예외보다 빈 결과 + 상태 메시지로 처리하고 현재 결과 컨텍스트를 비운다.

### 5.4 Read Model

- `RoadNameSearchResult`
  - `Highways`
  - `MatchKind(Exact|Partial|None)`
- `CurrentMapViewState`
- `TrafficLookupResult`
- `CctvLookupResult`

### 5.5 Aggregate

- 신규 Aggregate는 도입하지 않는다.
- 본 기능은 저장 상태를 추가하지 않는 조회성 유스케이스이므로, 기존 조회 컨텍스트와 `RoadNameSearchResult` 읽기 모델 조합으로 흐름을 구성한다.

---

## 6) 설계 결정

- 검색 데이터 소스는 `OpenStreet planet_osm_line`이 아니라 `vds + vds_loc`를 우선 사용한다.
- 이유: 검색 결과가 바로 기존 혼잡도/CCTV 후속 경로로 이어져야 하므로, 실제 VDS/노선번호가 있는 도로만 후보로 잡는 편이 맞다.
- 검색 매칭 포트는 별도로 둔다. 클릭용 `IOpenStreetQueryPort`는 유지한다.
- 형태소 확장 지점은 `검색어 확장/정규화 포트`로 둔다. 기본 구현은 no-op 또는 단순 normalize만 수행한다.
- 현재 기본 구현은 `DefaultRoadNameQueryExpanderAdapter`이며 trim, 공백 normalize, 원문 유지를 담당한다.
- 클릭과 검색의 공통 후속 경로는 `후보 고속도로가 정해진 뒤`부터 공유한다.
- 혼잡도 모드는 `매칭된 고속도로 집합` 전체를 대상으로 결과를 그린다.
- CCTV 모드는 기존 의미를 유지하기 위해 `최상위 매칭 1건`만 선택해 기존 단일 고속도로 CCTV 흐름으로 보낸다.

---

## 7) 아키텍처 변경 계획

### 7.1 App

- 새 커맨드 추가: `SearchRoadByNameCommand`
- 필드
  - `Query`
  - `Latitude`
  - `Longitude`
  - `MinLongitude`
  - `MinLatitude`
  - `MaxLongitude`
  - `MaxLatitude`
- 검색 시에도 현재 지도 중심/뷰 bounds를 함께 담아 기존 조회 컨텍스트와 맞춘다.
- 새 서비스 추가: `SearchRoadByNameService`
- 역할
  - 검색어 검증
  - bounds 정규화
  - 검색어 확장 포트 호출
  - 도로명 검색 포트 호출
  - exact/partial tier 결정
- 결과 DTO 추가 권장: `RoadNameSearchResult`
- 필드
  - `Highways`
  - `MatchKind(Exact|Partial|None)`

### 7.2 Port / Adapter

- 새 포트 추가: `IRoadNameHighwaySearchPort`
  - 책임: 현재 bounds 내부에서 검색어와 매칭되는 고속도로 후보를 반환
- 새 포트 추가: `IRoadNameQueryExpanderPort`
  - 책임: 형태소 플러그인/검색어 확장 지점. 기본 구현은 trim, 공백 normalize, 원문 유지
- 기본 어댑터 추가: `DefaultRoadNameQueryExpanderAdapter`
- 기본 어댑터 추가: `RoadNameHighwaySearchAdapter`
- 저장소 확장: `TrafficForm/Adapter/VdsRepository.cs`에 `bounds 내부 distinct highway 후보 조회` 메서드 추가
- 검색 우선순위는 exact tier가 있으면 exact만 반환, 없으면 partial tier를 반환한다.

### 7.3 Common downstream reuse

- `TrafficForm/App/RequestTrafficByPosService.cs`에 `고속도로 집합 + bounds -> traffic 결과` 공통 메서드를 분리한다.
- 기존 클릭 진입 메서드는 `인접 고속도로 resolve`만 담당하고, 이후는 공통 메서드를 호출하게 바꾼다.
- `TrafficForm/App/RequestCctvByPosService.cs`에 `선택된 highway + bounds -> CCTV 결과` 공통 메서드를 분리한다.
- 기존 클릭 진입 메서드는 `가장 가까운 highway 선택`까지만 담당하고, 이후는 공통 메서드를 호출하게 바꾼다.
- UI에도 공통 wrapper를 둔다.
  - `RunTrafficLookupAsync(...)`
  - `RunCctvLookupAsync(...)`
- 검색/클릭 모두 상태바, 중복 요청 차단, 최신 응답 반영, 패널 렌더링은 이 공통 wrapper를 타게 한다.

### 7.4 UI

- 검색 진입은 `TrafficForm/UI/Form1.cs`와 `TrafficForm/UI/Form1.Cctv.cs`에서 공통 wrapper 재사용 방향으로 반영됐다.
- 검색은 현재 지도 bounds를 읽어 `SearchRoadByNameCommand`로 전달한다.
- 검색은 `현재 우측 패널 모드`를 따른다.
- 검색 결과가 없으면 현재 결과 컨텍스트를 비우고 상태 메시지로 안내한다.

---

## 8) 코드 반영 매핑

- Composition Root
  - `TrafficForm/Program.cs`
- UI
  - `TrafficForm/UI/Form1.cs`
  - `TrafficForm/UI/Form1.Cctv.cs`
- App
  - `TrafficForm/App/UpdateSelectedPosTrafficInfoCommand.cs`
  - `TrafficForm/App/RequestTrafficByPosService.cs`
  - `TrafficForm/App/RequestCctvByPosService.cs`
  - `TrafficForm/App/SearchRoadByNameService.cs`
  - `TrafficForm/App/SearchRoadByNameCommand.cs`
- Port
  - `TrafficForm/Port/IRoadNameHighwaySearchPort.cs`
  - `TrafficForm/Port/IRoadNameQueryExpanderPort.cs`
- Adapter
  - `TrafficForm/Adapter/RoadNameHighwaySearchAdapter.cs`
  - `TrafficForm/Adapter/DefaultRoadNameQueryExpanderAdapter.cs`
  - `TrafficForm/Adapter/VdsRepository.cs`
- Test
  - `TestProject1/SearchRoadByNameServiceTest.cs`
  - `TestProject1/RequestTrafficServiceTest.cs`
  - `TestProject1/RequestCctvByPosServiceTest.cs`
  - `TestProject1/DiWiringTest.cs`
- Docs
  - `docs/road-name-search-usecase-eventstorming.md`
  - `docs/usecase-spec.md`
  - `README.md`

---

## 9) 검증 결과

- `python3 .agents/skills/docs-verify/scripts/run.py`
  - 결과: 실행됨, 통과
  - 상세: 문서 검증이 실행되어 통과했다.
- `dotnet build TrafficSolution.slnx`
  - 결과: 실행됨, 실패 (`NETSDK1100`)
  - 상세: 현재 환경에서 `NETSDK1100`으로 빌드가 실패했다.
- `dotnet test TrafficSolution.slnx`
  - 결과: 실행됨, 실패 (`NETSDK1100`)
  - 상세: 현재 환경에서 `NETSDK1100`으로 테스트가 실패했다.

### 9.1 테스트 반영 파일

- `TestProject1/SearchRoadByNameServiceTest.cs`
- `TestProject1/RequestTrafficServiceTest.cs`
- `TestProject1/RequestCctvByPosServiceTest.cs`
- `TestProject1/DiWiringTest.cs`

---

## 10) 문서 반영 범위

- 신규 설계 문서 추가: `docs/road-name-search-usecase-eventstorming.md`
- `docs/usecase-spec.md` 업데이트
  - 신규 식별자 `UC-SCH-001 도로명 검색 기반 조회` 추가
  - 혼잡도/CCTV 기존 UC에서 `검색 진입도 동일 downstream 사용` 링크 연결
- `README.md` 문서 링크 섹션 업데이트
- 계획 문서 경로는 현재 repo에 `completed/...`만 존재하며, active/completed 정합성 검토가 남아 있다.

---

## 11) 가정 / 비목표

### 11.1 Assumptions

- 검색은 자동완성 UI 없이 `입력 후 실행` 방식으로 충분하다.
- 검색은 현재 map bounds 기준만 사용한다. 별도 전체 검색 모드는 이번 범위 밖이다.
- CCTV 검색 결과는 여러 도로를 동시에 보여주지 않고, 최고 우선순위 도로 1건으로 고정한다.
- 예외 신설은 우선 피한다. no-result는 예외보다 빈 결과 + 상태 메시지로 처리한다.

### 11.2 Remaining Risks / Follow-ups

- 현재 워크트리는 중단 시점의 부분 구현 상태라 컴파일 가능 여부가 확인되지 않았다.
- `README.md`와 설계 문서는 현재 repo에 존재하는 `docs/exec-plans/completed/road-name-search/plan.md`를 가리키며, raw execute output의 active 경로는 역사적 참조다.
- 기존 `TrafficForm/App/RequestTrafficByPosService.cs`의 `NotImplementedException` 부채는 이번 작업의 주목적이 아니다. 검색 기능 구현 중 범위를 불필요하게 넓히지 않는다.

### 11.2 Non-Goals

- 형태소 분석기 실제 연동
- 오타 교정/퍼지 검색
- 검색 결과 드롭다운 추천 목록
- 지도 모드 체계 재설계
- UI 자동화 테스트 추가

---

## 12) 완료 기준

- 사용자가 좌측 검색창에 도로명을 입력하고 실행하면, 현재 map bounds 내부 도로만 검색된다.
- exact match가 있으면 exact 결과만 사용한다.
- exact가 없을 때만 partial like 결과를 사용한다.
- 혼잡도 모드에서는 기존 클릭 결과와 동일한 카드/마커/세그먼트 표시를 재사용한다.
- CCTV 모드에서는 기존 클릭 결과와 동일한 CCTV 카드/마커 표시를 재사용한다.
- 클릭과 검색 모두 공통 downstream 메서드와 공통 UI wrapper를 사용한다.
- 형태소 확장용 포트가 존재하고, 기본 expander adapter가 trim/공백 normalize를 수행한다.
- 테스트는 `TestProject1`에만 추가된다.
- 설계 문서와 `docs/usecase-spec.md`가 함께 갱신된다.

---

## 13) 후속 개선 포인트

- Windows 또는 `Microsoft.WindowsDesktop.App 10.0` 런타임이 있는 환경에서 `dotnet test` 재실행
- `TrafficSolution.slnx` solution-level build 실패 원인 재확인
- AGENTS 규칙상 build 이후 앱 실행은 하지 않았으므로 검색 UI와 실제 지도 연동은 수동 점검 필요
- 형태소 분석기 또는 검색어 확장 포트의 실제 플러그인 구현
- 오타 교정/퍼지 검색 정책 추가 여부 검토
- 검색 결과 추천 목록 또는 자동완성 UI 분리
- 현재 bounds 전용 검색과 전체 검색 모드의 분리 여부 검토
