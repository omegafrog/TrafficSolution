# TrafficSolution Use Case Specification

이 문서는 현재 구현된 기능을 유스케이스 식별자 기준으로 정리합니다.
각 유스케이스는 클라이언트 관점 설명, 데이터 흐름, 로직 흐름을 PlantUML 시퀀스 다이어그램으로 제공합니다.
구현 전 설계 단계 기능은 별도 식별자 표와 설계 문서 링크로 연결합니다.

## 식별자 목록

| 식별자 | 유스케이스 명 | 관련 주요 코드 |
|---|---|---|
| [UC-CMN-001](#uc-cmn-001) | 앱 실행 및 지도 준비 | `TrafficForm/Program.cs`, `TrafficForm/UI/Form1.cs` |
| [UC-MODE-001](#uc-mode-001) | 지도 모드 전환 | `TrafficForm/UI/Form1.cs` |
| [UC-MODE-002](#uc-mode-002) | 우측 패널 모드 전환 | `TrafficForm/UI/Form1.Cctv.cs` |
| [UC-TRF-001](#uc-trf-001) | 좌표 기반 혼잡도 조회 | `TrafficForm/UI/Form1.cs`, `TrafficForm/App/RequestTrafficByPosService.cs` |
| [UC-TRF-002](#uc-trf-002) | VDS 결과 시각화/동기화 | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/HighwayListControl.cs` |
| [UC-TRF-003](#uc-trf-003) | 도로 구간 혼잡도 색상 시각화 | `TrafficForm/UI/Form1.cs`, `TrafficForm/Domain/TrafficLevelPolicy.cs`, `TrafficForm/Adapter/VdsRepository.cs` |
| [UC-CTV-001](#uc-ctv-001) | CCTV 모드 기반 조회 | `TrafficForm/UI/Form1.Cctv.cs`, `TrafficForm/App/RequestCctvByPosService.cs` |
| [UC-CTV-002](#uc-ctv-002) | CCTV 상세 재생 | `TrafficForm/UI/Form1.Cctv.cs`, `TrafficForm/UI/CctvPlayerPopupForm.cs` |
| [UC-CTV-003](#uc-ctv-003) | CCTV 선택 동기화 | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs`, `TrafficForm/UI/CctvListControl.cs` |
| [UC-SCH-001](#uc-sch-001) | 도로명 검색 기반 조회 | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs`, `TrafficForm/App/SearchRoadByNameService.cs`, `TrafficForm/Adapter/RoadNameHighwaySearchAdapter.cs` |
| [UC-OPS-001](#uc-ops-001) | 중복 요청 방지 및 최신 응답만 반영 | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs` |
| [UC-OPS-002](#uc-ops-002) | 좌표/경계 검증 및 정규화 | `TrafficForm/App/UpdateSelectedPosTrafficInfoCommand.cs`, `TrafficForm/App/UpdateSelectedPosCctvInfoCommand.cs`, `TrafficForm/App/RequestTrafficByPosService.cs`, `TrafficForm/App/RequestCctvByPosService.cs`, `TestProject1/RequestTrafficServiceTest.cs`, `TestProject1/RequestCctvByPosServiceTest.cs` |
| [UC-OPS-003](#uc-ops-003) | 조회 상태 메시지/진행 인디케이터 갱신 | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs` |

---

<a id="uc-mode-002"></a>
## UC-MODE-002 우측 패널 모드 전환

### 1) 클라이언트 기준 상세 설명

- 사용자는 우측 패널 모드를 `혼잡도 모드/CCTV 모드` 중 하나로 변경한다.
- 시스템은 모드 값만 변경하며, 모드 전환 시점에는 기존에 생성된 하이라이트/마커를 초기화한다.
- 모드 전환 자체로 새로운 조회를 시작하지 않으며, 이후 지도 클릭/선택 동작에 따라 데이터가 다시 표시된다.
- 완료 기준: 모드 전환 직후 이전 상태의 하이라이트/마커가 제거되고, 새 데이터 표시는 후속 조회/선택 이후에만 발생한다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 1](images/plantuml/usecase-spec-01.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 2](images/plantuml/usecase-spec-02.svg)

---

<a id="uc-cmn-001"></a>
## UC-CMN-001 앱 실행 및 지도 준비

### 1) 클라이언트 기준 상세 설명

- 사용자는 앱을 실행하고 지도가 준비될 때까지 대기한다.
- 시스템은 UI를 초기화하고 WebView2 지도 로딩 완료 시 상태 메시지를 갱신한다.
- 완료 기준: 상태바에 `지도가 준비되었습니다.`가 표시된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 3](images/plantuml/usecase-spec-03.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 4](images/plantuml/usecase-spec-04.svg)

---

<a id="uc-mode-001"></a>
## UC-MODE-001 지도 모드 전환

### 1) 클라이언트 기준 상세 설명

- 사용자는 툴바에서 지도 모드를 `일반 모드` 또는 `주변 고속도로 선택 모드`로 전환한다.
- 시스템은 선택 모드 여부를 지도 커서/선택 상태에 반영한다.
- 완료 기준: `주변 고속도로 선택 모드`에서만 좌표 클릭 조회가 활성화된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 5](images/plantuml/usecase-spec-05.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 6](images/plantuml/usecase-spec-06.svg)

---

<a id="uc-trf-001"></a>
## UC-TRF-001 좌표 기반 혼잡도 조회

### 1) 클라이언트 기준 상세 설명

- 사용자는 지도 모드를 `주변 고속도로 선택 모드`로 바꾼 뒤 지도를 클릭한다.
- 클라이언트는 클릭 좌표와 현재 지도 bounds(min/max lat/lon)를 함께 전송한다.
- 시스템은 인접 고속도로를 식별하고, 각 고속도로별 VDS 혼잡도 정보를 조회한다.
- 조회 결과는 우측 패널 카드와 지도 마커/세그먼트로 표시된다.
- 관련 구현: 도로명 검색 진입도 동일 downstream을 재사용하도록 [UC-SCH-001](#uc-sch-001)이 반영되어 있다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 7](images/plantuml/usecase-spec-07.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 8](images/plantuml/usecase-spec-08.svg)

---

<a id="uc-trf-002"></a>
## UC-TRF-002 VDS 결과 시각화/동기화

### 1) 클라이언트 기준 상세 설명

- 사용자는 혼잡도 조회 결과를 지도와 우측 카드 리스트에서 동시에 확인한다.
- 지도의 VDS 마커를 클릭하면 해당 카드가 하이라이트된다.
- 선택 상태를 해제하면 하이라이트도 함께 해제된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 9](images/plantuml/usecase-spec-09.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 10](images/plantuml/usecase-spec-10.svg)

---

<a id="uc-ctv-001"></a>
## UC-CTV-001 CCTV 모드 기반 조회

### 1) 클라이언트 기준 상세 설명

- 사용자는 좌측에서 우측 패널 모드를 `CCTV 모드`로 전환한다.
- 지도 클릭 시 선택 좌표 기준으로 가장 가까운 고속도로를 고르고 CCTV 후보를 조회한다.
- 조회된 CCTV 후보는 선택 고속도로 반경 1km 이내 + 고속도로명 유사 조건으로 필터링되어 우측 카드와 지도 CCTV 마커로 표시된다.
- 관련 구현: 도로명 검색 진입도 동일 downstream을 재사용하도록 [UC-SCH-001](#uc-sch-001)이 반영되어 있다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 11](images/plantuml/usecase-spec-11.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 12](images/plantuml/usecase-spec-12.svg)

---

<a id="uc-ctv-002"></a>
## UC-CTV-002 CCTV 상세 재생

### 1) 클라이언트 기준 상세 설명

- 사용자가 CCTV 카드를 클릭하면 지도에서 해당 CCTV 마커를 강조한다.
- 스트림 URL을 검증한 뒤 팝업 플레이어를 띄워 실시간 영상을 재생한다.
- 재생 창이 이미 열려 있으면 중복 실행을 막고 상태 메시지로 안내한다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 13](images/plantuml/usecase-spec-13.svg)

---

<a id="uc-trf-003"></a>
## UC-TRF-003 도로 구간 혼잡도 색상 시각화

### 1) 클라이언트 기준 상세 설명

- 사용자는 조회된 도로 구간의 혼잡도 레벨을 색상으로 확인한다.
- 시스템은 VDS 책임 구간 좌표를 가져와 혼잡도 레벨 색상으로 세그먼트를 그린다.
- 완료 기준: 구간이 레벨별 색상(`원활/보통/혼잡/정체`)으로 지도에 표시된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 14](images/plantuml/usecase-spec-14.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 15](images/plantuml/usecase-spec-15.svg)

---

<a id="uc-ctv-003"></a>
## UC-CTV-003 CCTV 선택 동기화

### 1) 클라이언트 기준 상세 설명

- 사용자는 CCTV 카드 또는 지도 CCTV 마커를 선택한다.
- 시스템은 카드 하이라이트, 스크롤, 지도 마커 포커스를 동기화한다.
- 완료 기준: 동일 CCTV가 지도와 목록에서 동시에 강조된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 16](images/plantuml/usecase-spec-16.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 17](images/plantuml/usecase-spec-17.svg)

---

<a id="uc-ops-001"></a>
## UC-OPS-001 중복 요청 방지 및 최신 응답만 반영

### 1) 클라이언트 기준 상세 설명

- 사용자가 조회 중 같은 동작을 반복하면 시스템은 중복 요청을 제한한다.
- 트래픽/CCTV 조회는 각각 요청 버전을 증가시키고, 최신 요청 버전만 UI에 반영한다.
- 완료 기준: 지연된 이전 응답이 도착해도 화면은 최신 요청 결과만 유지된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 18](images/plantuml/usecase-spec-18.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 19](images/plantuml/usecase-spec-19.svg)

---

<a id="uc-ops-002"></a>
## UC-OPS-002 좌표/경계 검증 및 정규화

### 1) 클라이언트 기준 상세 설명

- 사용자가 좌표를 선택하면 시스템은 좌표/경계값 유효성을 확인한다.
- 혼잡도 조회는 `TrafficForm/App/UpdateSelectedPosTrafficInfoCommand.cs`와 `TrafficForm/App/RequestTrafficByPosService.cs` 경로에서 bounds를 정규화하고 남한 범위로 clamp 처리한다.
- CCTV 조회는 `TrafficForm/App/UpdateSelectedPosCctvInfoCommand.cs`와 `TrafficForm/App/RequestCctvByPosService.cs` 경로에서 동일한 bounds 정규화/clamp 규칙을 적용한다.
- 완료 기준: 잘못된 좌표는 예외/실패 메시지로 처리되고, 비정상 경계 입력은 traffic/CCTV 조회 모두에서 정규화되어 조회된다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 20](images/plantuml/usecase-spec-20.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 21](images/plantuml/usecase-spec-21.svg)

---

<a id="uc-ops-003"></a>
## UC-OPS-003 조회 상태 메시지/진행 인디케이터 갱신

### 1) 클라이언트 기준 상세 설명

- 사용자는 상태바를 통해 조회 시작/진행/완료/실패를 확인한다.
- 시스템은 단계별로 `SetStatusMessage`와 진행 인디케이터를 갱신한다.
- 완료 기준: 사용자가 현재 조회 상태를 상태바에서 즉시 구분할 수 있다.

### 2) 데이터 흐름 (Sequence Diagram)

![Rendered diagram 22](images/plantuml/usecase-spec-22.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 23](images/plantuml/usecase-spec-23.svg)

### 3) 로직 흐름 (Sequence Diagram)

![Rendered diagram 24](images/plantuml/usecase-spec-24.svg)

---

<a id="uc-sch-001"></a>
## UC-SCH-001 도로명 검색 기반 조회

### 1) 클라이언트 기준 상세 설명

- 사용자는 좌측 검색창에 도로명을 입력하고 검색을 실행한다.
- 시스템은 현재 지도 center/bounds를 읽고, bounds 내부 `vds + vds_loc` 기준으로 exact > partial 순서로 도로를 찾는다.
- 현재 우측 패널 모드가 `혼잡도`면 매칭된 고속도로 집합 전체로 VDS 조회를 이어가고, `CCTV`면 최상위 1건으로 CCTV 조회를 이어간다.
- 검색 결과가 없으면 현재 모드의 결과 컨텍스트를 비우고 상태 메시지로 안내한다.
- 현재 worktree 기준 구현/문서 반영은 존재하고, `python3 .agents/skills/docs-verify/scripts/run.py`는 실행되어 통과했으며 `dotnet build TrafficSolution.slnx`와 `dotnet test TrafficSolution.slnx`는 `NETSDK1100`으로 실패했다.

### 2) 관련 구현 / 설계 문서

- 구현 코드:
  - `TrafficForm/UI/Form1.cs`
  - `TrafficForm/UI/Form1.Cctv.cs`
  - `TrafficForm/App/RequestTrafficByPosService.cs`
  - `TrafficForm/App/RequestCctvByPosService.cs`
  - `TrafficForm/App/SearchRoadByNameService.cs`
  - `TrafficForm/App/SearchRoadByNameCommand.cs`
  - `TrafficForm/Port/IRoadNameHighwaySearchPort.cs`
  - `TrafficForm/Port/IRoadNameQueryExpanderPort.cs`
  - `TrafficForm/Adapter/DefaultRoadNameQueryExpanderAdapter.cs`
  - `TrafficForm/Adapter/RoadNameHighwaySearchAdapter.cs`
  - `TrafficForm/Adapter/VdsRepository.cs`
- 설계 문서:
  - [`docs/road-name-search-usecase-eventstorming.md`](road-name-search-usecase-eventstorming.md)
  - [`docs/exec-plans/completed/road-name-search/plan.md`](exec-plans/completed/road-name-search/plan.md)
    - 현재 repo에 존재하는 stop-state 문서 경로다. raw execute output은 `docs/exec-plans/active/road-name-search/plan.md` 기준 partial stop-state를 가리켰다.
  - [`docs/exec-plans/completed/road-name-search/design/detailed-design.md`](exec-plans/completed/road-name-search/design/detailed-design.md)

### 3) 핵심 규칙

- 검색 대상은 항상 현재 map bounds 내부로 제한한다.
- exact match가 있으면 exact 결과만 사용하고, 없을 때만 partial 결과를 사용한다.
- 혼잡도 모드는 매칭된 고속도로 집합 전체를 사용하고, CCTV 모드는 최상위 매칭 1건만 사용한다.
- `SearchRoadByNameService`는 `IRoadNameQueryExpanderPort`와 `IRoadNameHighwaySearchPort`를 통해 검색 체인을 구성한다.
- 혼잡도 조회는 `RequestTrafficByPosService.GetTrafficByHighwaysAsync(...)`, CCTV 조회는 `RequestCctvByPosService.GetHighwayCctvAsync(...)` 공통 메서드로 이어진다.
- 클릭과 검색은 후보 고속도로가 정해진 뒤부터 공통 downstream/UI wrapper를 재사용한다.
