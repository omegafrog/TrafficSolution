# TrafficSolution Use Case Specification

이 문서는 현재 구현된 기능을 유스케이스 식별자 기준으로 정리합니다.
각 유스케이스는 클라이언트 관점 설명, 데이터 흐름, 로직 흐름을 PlantUML 시퀀스 다이어그램으로 제공합니다.

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
| [UC-OPS-001](#uc-ops-001) | 중복 요청 방지 및 최신 응답만 반영 | `TrafficForm/UI/Form1.cs`, `TrafficForm/UI/Form1.Cctv.cs` |
| [UC-OPS-002](#uc-ops-002) | 좌표/경계 검증 및 정규화 | `TrafficForm/App/UpdateSelectedPosCctvInfoCommand.cs`, `TrafficForm/App/RequestCctvByPosService.cs`, `TrafficForm/App/RequestTrafficByPosService.cs`, `TestProject1/RequestTrafficServiceTest.cs`, `TestProject1/RequestCctvByPosServiceTest.cs` |
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

```plantuml
@startuml
title UC-MODE-002 Data Flow

actor User
participant Form1

User -> Form1 : 패널 모드 변경(혼잡도/CCTV)
Form1 -> Form1 : SetRightPanelModeAsync(nextMode)
Form1 -> Form1 : 기존 하이라이트/마커 초기화
Form1 --> User : 모드 전환 상태 메시지
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-MODE-002 Logic Flow

actor User
participant Form1

User -> Form1 : 모드 선택
Form1 -> Form1 : 현재 모드와 비교
alt 동일 모드
  Form1 -> Form1 : 변경 없이 종료
else 다른 모드
  Form1 -> Form1 : 모드 값 갱신
  Form1 -> Form1 : 기존 하이라이트/마커 초기화
  Form1 -> Form1 : 외부 조회 없이 대기 상태 유지
end
@enduml
```

---

<a id="uc-cmn-001"></a>
## UC-CMN-001 앱 실행 및 지도 준비

### 1) 클라이언트 기준 상세 설명

- 사용자는 앱을 실행하고 지도가 준비될 때까지 대기한다.
- 시스템은 UI를 초기화하고 WebView2 지도 로딩 완료 시 상태 메시지를 갱신한다.
- 완료 기준: 상태바에 `지도가 준비되었습니다.`가 표시된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CMN-001 Data Flow

actor User
participant Program
participant Form1
participant "WebView2" as WebView

User -> Program : 앱 실행
Program -> Form1 : Form 생성/DI 주입
Form1 -> WebView : 지도 HTML 로딩
WebView --> Form1 : NavigationCompleted
Form1 --> User : 지도 준비 상태 표시
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CMN-001 Logic Flow

actor User
participant Form1

User -> Form1 : 앱 시작
Form1 -> Form1 : 상태바/모드 UI 초기화
Form1 -> Form1 : InitializeWebView
alt 지도 로딩 성공
  Form1 -> Form1 : UpdateMapCursorAsync
  Form1 --> User : "지도가 준비되었습니다."
else 지도 로딩 실패
  Form1 --> User : "지도 로딩에 실패했습니다."
end
@enduml
```

---

<a id="uc-mode-001"></a>
## UC-MODE-001 지도 모드 전환

### 1) 클라이언트 기준 상세 설명

- 사용자는 툴바에서 지도 모드를 `일반 모드` 또는 `주변 고속도로 선택 모드`로 전환한다.
- 시스템은 선택 모드 여부를 지도 커서/선택 상태에 반영한다.
- 완료 기준: `주변 고속도로 선택 모드`에서만 좌표 클릭 조회가 활성화된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-MODE-001 Data Flow

actor User
participant Form1
participant "WebView2/Leaflet" as Map

User -> Form1 : 지도 모드 변경
Form1 -> Form1 : SetMapInteractionModeAsync
Form1 -> Map : setPosSelectionMode(true/false)
Form1 --> User : 모드 안내 상태 메시지
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-MODE-001 Logic Flow

actor User
participant Form1

User -> Form1 : 모드 선택
Form1 -> Form1 : _mapInteractionMode 갱신
Form1 -> Form1 : UpdateMapCursorAsync
alt NearbyHighwayLookup
  Form1 --> User : 좌표 선택 모드 메시지
else None
  Form1 --> User : 조회 비활성화 메시지
end
@enduml
```

---

<a id="uc-trf-001"></a>
## UC-TRF-001 좌표 기반 혼잡도 조회

### 1) 클라이언트 기준 상세 설명

- 사용자는 지도 모드를 `주변 고속도로 선택 모드`로 바꾼 뒤 지도를 클릭한다.
- 클라이언트는 클릭 좌표와 현재 지도 bounds(min/max lat/lon)를 함께 전송한다.
- 시스템은 인접 고속도로를 식별하고, 각 고속도로별 VDS 혼잡도 정보를 조회한다.
- 조회 결과는 우측 패널 카드와 지도 마커/세그먼트로 표시된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-TRF-001 Data Flow

actor User
participant "Form1(WebView2)" as Form1
participant "RequestTrafficByPosService" as TrafficSvc
participant "IOpenStreetQueryPort" as OpenStreetPort
participant "OpenStreetQueryAdapter" as OpenStreetAdapter
participant "OpenStreetDbRepository" as OpenStreetRepo
database "Geo Postgres" as GeoDb
participant "IPublicTrafficApiPort" as TrafficApiPort
participant "PublicTrafficApiAdapter" as TrafficAdapter
participant "ITS VDS API" as VdsApi
participant "VdsRepository" as VdsRepo

User -> Form1 : 지도 클릭(lat, lon, bounds)
Form1 -> TrafficSvc : GetAdjacentHighWays(command)

TrafficSvc -> OpenStreetPort : GetAdjacentHighWays(location)
OpenStreetPort -> OpenStreetAdapter
OpenStreetAdapter -> OpenStreetRepo : findAdjacentHighways(lat, lon)
OpenStreetRepo -> GeoDb : 공간 쿼리(motorway/trunk)
GeoDb --> OpenStreetRepo : 인접 고속도로 목록
OpenStreetRepo --> OpenStreetAdapter
OpenStreetAdapter --> OpenStreetPort
OpenStreetPort --> TrafficSvc

loop 고속도로별 VDS 조회
  TrafficSvc -> TrafficApiPort : GetTrafficResult(highwayNo, bounds)
  TrafficApiPort -> TrafficAdapter
  TrafficAdapter -> VdsApi : vdsInfo API 호출
  VdsApi --> TrafficAdapter : 속도/점유율/볼륨
  TrafficAdapter -> VdsRepo : findVdsIdIn + findResponsibilitySegments
  VdsRepo -> GeoDb : vds / vds_loc 조회
  GeoDb --> VdsRepo : 좌표/구간
  VdsRepo --> TrafficAdapter
  TrafficAdapter --> TrafficApiPort
  TrafficApiPort --> TrafficSvc
end

TrafficSvc --> Form1 : Dictionary<int, List<VdsTrafficResult>>
Form1 -> Form1 : 카드/마커/세그먼트 렌더링
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-TRF-001 Logic Flow

actor User
participant Form1
participant TrafficSvc

User -> Form1 : 좌표 클릭
Form1 -> Form1 : 모드 확인(NearbyHighwayLookup)
alt 모드 아님
  Form1 --> User : 조회 비활성 상태 메시지
else 모드 정상
  Form1 -> Form1 : 요청 버전 증가, 로딩 상태 ON
  Form1 -> TrafficSvc : GetAdjacentHighWays(command)
  TrafficSvc -> TrafficSvc : 좌표 유효성 확인
  TrafficSvc -> TrafficSvc : 고속도로별 결과 수집/필터링
  TrafficSvc --> Form1 : 통합 결과
  Form1 -> Form1 : 최신 결과 캐시
  Form1 -> Form1 : 우측 패널 + 지도 갱신
  Form1 --> User : 조회 완료 상태 메시지
end
@enduml
```

---

<a id="uc-trf-002"></a>
## UC-TRF-002 VDS 결과 시각화/동기화

### 1) 클라이언트 기준 상세 설명

- 사용자는 혼잡도 조회 결과를 지도와 우측 카드 리스트에서 동시에 확인한다.
- 지도의 VDS 마커를 클릭하면 해당 카드가 하이라이트된다.
- 선택 상태를 해제하면 하이라이트도 함께 해제된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-TRF-002 Data Flow

actor User
participant "Leaflet(JS in WebView)" as Leaflet
participant Form1
participant "HighwayListControl" as HighwayCard

User -> Leaflet : VDS 마커 클릭
Leaflet -> Form1 : web message(type=vds-selected, id)
Form1 -> Form1 : controlMap에서 카드 조회
Form1 -> HighwayCard : SetHighlighted(true)
Form1 --> User : 카드 강조/스크롤 이동

User -> Leaflet : 빈 지도 클릭
Leaflet -> Form1 : web message(type=vds-selection-cleared)
Form1 -> HighwayCard : ClearHighlight()
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-TRF-002 Logic Flow

actor User
participant Form1

User -> Form1 : vds-selected 이벤트 유입
Form1 -> Form1 : 현재 패널 모드 확인(혼잡도)
alt 혼잡도 모드 아님
  Form1 -> Form1 : 이벤트 무시
else 혼잡도 모드
  Form1 -> Form1 : vdsId 파싱
  alt vdsId 유효 + 카드 존재
    Form1 -> Form1 : 카드 선택/강조/요약 갱신
  else
    Form1 -> Form1 : 선택 해제
  end
end
@enduml
```

---

<a id="uc-ctv-001"></a>
## UC-CTV-001 CCTV 모드 기반 조회

### 1) 클라이언트 기준 상세 설명

- 사용자는 좌측에서 우측 패널 모드를 `CCTV 모드`로 전환한다.
- 지도 클릭 시 선택 좌표 기준으로 가장 가까운 고속도로를 고르고 CCTV 후보를 조회한다.
- 조회된 CCTV 후보는 선택 고속도로 반경 1km 이내 + 고속도로명 유사 조건으로 필터링되어 우측 카드와 지도 CCTV 마커로 표시된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CTV-001 Data Flow

actor User
participant Form1
participant "RequestCctvByPosService" as CctvSvc
participant "IOpenStreetQueryPort" as OpenStreetPort
participant "IPublicTrafficApiPort" as TrafficApiPort
participant "ICctvApiPort" as CctvApiPort
participant "ITS CCTV API" as CctvApi

User -> Form1 : 패널 모드=CCTV, 지도 클릭
Form1 -> CctvSvc : GetNearbyHighwayCctv(command)

CctvSvc -> OpenStreetPort : 인접 고속도로 조회
OpenStreetPort --> CctvSvc : 고속도로 후보

loop 고속도로별
  CctvSvc -> TrafficApiPort : VDS 결과 조회(bounds)
  TrafficApiPort --> CctvSvc : VDS 위치 목록
end

CctvSvc -> CctvApiPort : GetCctvInfo(bounds)
CctvApiPort -> CctvApi : cctvInfo API 호출
CctvApi --> CctvApiPort : CCTV 후보 목록
CctvApiPort --> CctvSvc

CctvSvc -> CctvSvc : 선택 고속도로 반경 1km + 고속도로명 유사 필터링
CctvSvc --> Form1 : HighwayCctvSelection
Form1 -> Form1 : CCTV 카드/마커 렌더링
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CTV-001 Logic Flow

actor User
participant Form1
participant CctvSvc

User -> Form1 : 좌표 클릭
Form1 -> Form1 : 패널 모드 확인(CCTV)
alt CCTV 모드 아님
  Form1 -> Form1 : 혼잡도 조회 분기로 전환
else CCTV 모드
  Form1 -> Form1 : 요청 버전 증가, 로딩 상태 ON
  Form1 -> CctvSvc : GetNearbyHighwayCctv(command)
  CctvSvc -> CctvSvc : 좌표 범위 검증 + bounds normalize
  CctvSvc -> CctvSvc : 인접 고속도로 선정 + 최단거리 고속도로 선택
  CctvSvc -> CctvSvc : 1km 거리 + 고속도로명 유사도 규칙으로 CCTV 필터링
  CctvSvc --> Form1 : 선택 고속도로 + CCTV 목록
  Form1 -> Form1 : 최신 CCTV 캐시 + 패널 갱신
end
@enduml
```

---

<a id="uc-ctv-002"></a>
## UC-CTV-002 CCTV 상세 재생

### 1) 클라이언트 기준 상세 설명

- 사용자가 CCTV 카드를 클릭하면 지도에서 해당 CCTV 마커를 강조한다.
- 스트림 URL을 검증한 뒤 팝업 플레이어를 띄워 실시간 영상을 재생한다.
- 재생 창이 이미 열려 있으면 중복 실행을 막고 상태 메시지로 안내한다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CTV-002 Data Flow

actor User
participant "CctvListControl" as CctvCard
participant Form1
participant "WebView2/Leaflet" as Map
participant "CctvPlayerPopupForm" as Popup

User -> CctvCard : 카드 클릭
CctvCard -> Form1 : CardClicked(cctvId, streamUrl)
Form1 -> Map : highlightCctvMarker(cctvId)
Form1 -> Form1 : URL 유효성 검증
Form1 -> Popup : ShowDialog(displayName, streamUrl)
Popup --> User : 실시간 CCTV 재생 화면
@enduml
```

---

<a id="uc-trf-003"></a>
## UC-TRF-003 도로 구간 혼잡도 색상 시각화

### 1) 클라이언트 기준 상세 설명

- 사용자는 조회된 도로 구간의 혼잡도 레벨을 색상으로 확인한다.
- 시스템은 VDS 책임 구간 좌표를 가져와 혼잡도 레벨 색상으로 세그먼트를 그린다.
- 완료 기준: 구간이 레벨별 색상(`원활/보통/혼잡/정체`)으로 지도에 표시된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-TRF-003 Data Flow

actor User
participant Form1
participant "TrafficLevelPolicy" as Policy
participant "VdsRepository" as VdsRepo
database "Geo Postgres" as GeoDb

User -> Form1 : 혼잡도 결과 확인
Form1 -> VdsRepo : 책임 구간 좌표 조회
VdsRepo -> GeoDb : vds_loc 조회
GeoDb --> VdsRepo : 구간 좌표 목록
Form1 -> Policy : 레벨별 색상 계산
Form1 --> User : 구간 세그먼트 색상 표시
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-TRF-003 Logic Flow

actor User
participant Form1

User -> Form1 : 조회 결과 화면 확인
Form1 -> Form1 : clearSegments
loop VDS 결과별
  Form1 -> Form1 : responsibility segment 존재 여부 확인
  Form1 -> Form1 : TrafficLevelPolicy.GetColorHex
  Form1 -> Form1 : addSegment(points, color)
end
@enduml
```

---

<a id="uc-ctv-003"></a>
## UC-CTV-003 CCTV 선택 동기화

### 1) 클라이언트 기준 상세 설명

- 사용자는 CCTV 카드 또는 지도 CCTV 마커를 선택한다.
- 시스템은 카드 하이라이트, 스크롤, 지도 마커 포커스를 동기화한다.
- 완료 기준: 동일 CCTV가 지도와 목록에서 동시에 강조된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CTV-003 Data Flow

actor User
participant "WebView2/Leaflet" as Map
participant Form1
participant "CctvListControl" as Card

User -> Map : CCTV 마커 클릭
Map -> Form1 : cctv-selected(id)
Form1 -> Card : SelectCctvControl
Form1 --> User : 카드 강조/스크롤

User -> Card : 카드 클릭
Card -> Form1 : CardClicked(id)
Form1 -> Map : highlightCctvMarker(id)
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CTV-003 Logic Flow

actor User
participant Form1

User -> Form1 : cctv-selected 또는 CardClicked
Form1 -> Form1 : _cctvControlMap 조회
alt 매핑된 카드 존재
  Form1 -> Form1 : SelectCctvControl(control)
else
  Form1 -> Form1 : SelectCctvControl(null)
end
Form1 -> Form1 : 지도 마커 강조 동기화
@enduml
```

---

<a id="uc-ops-001"></a>
## UC-OPS-001 중복 요청 방지 및 최신 응답만 반영

### 1) 클라이언트 기준 상세 설명

- 사용자가 조회 중 같은 동작을 반복하면 시스템은 중복 요청을 제한한다.
- 트래픽/CCTV 조회는 각각 요청 버전을 증가시키고, 최신 요청 버전만 UI에 반영한다.
- 완료 기준: 지연된 이전 응답이 도착해도 화면은 최신 요청 결과만 유지된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-OPS-001 Data Flow

actor User
participant Form1
participant TrafficSvc
participant CctvSvc

User -> Form1 : 연속 조회 클릭
Form1 -> Form1 : Interlocked.Increment(requestVersion)
Form1 -> TrafficSvc : 트래픽 조회(또는)
Form1 -> CctvSvc : CCTV 조회
TrafficSvc --> Form1 : 결과
Form1 -> Form1 : requestVersion 비교 후 최신만 반영
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-OPS-001 Logic Flow

actor User
participant Form1

User -> Form1 : pos-selected 이벤트 반복
alt 동일 모드에서 이미 조회 중
  Form1 --> User : "이미 조회 중" 메시지
else
  Form1 -> Form1 : 조회 시작 플래그 ON
  Form1 -> Form1 : 요청 버전 증가
  Form1 -> Form1 : 응답 시 버전/모드 검증
  Form1 -> Form1 : 최신 요청만 렌더링
end
@enduml
```

---

<a id="uc-ops-002"></a>
## UC-OPS-002 좌표/경계 검증 및 정규화

### 1) 클라이언트 기준 상세 설명

- 사용자가 좌표를 선택하면 시스템은 좌표/경계값 유효성을 확인한다.
- CCTV 조회는 bounds를 정규화하고 남한 범위로 clamp 처리한다.
- 완료 기준: 잘못된 좌표는 예외/실패 메시지로 처리되고, 비정상 경계 입력은 정규화되어 조회된다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-OPS-002 Data Flow

actor User
participant Form1
participant RequestTrafficByPosService as TrafficSvc
participant RequestCctvByPosService as CctvSvc
participant UpdateSelectedPosCctvInfoCommand as CctvCmd

User -> Form1 : 좌표 클릭 + bounds
Form1 -> CctvCmd : NormalizeBounds()
Form1 -> TrafficSvc : 트래픽 조회 명령
Form1 -> CctvSvc : CCTV 조회 명령
TrafficSvc --> Form1 : 유효성 오류 또는 결과
CctvSvc --> Form1 : 유효성 오류 또는 결과
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-OPS-002 Logic Flow

actor User
participant Form1
participant CctvSvc

User -> Form1 : 좌표 선택
Form1 -> Form1 : 좌표 JSON 파싱/정규화
alt 파싱 실패
  Form1 --> User : 조회 실패 메시지
else 파싱 성공
  Form1 -> CctvSvc : ValidateSelectedPoint + NormalizeBounds
  alt 범위 초과
    CctvSvc --> User : PointOutOfRangeException 경로
  else 정상
    CctvSvc --> Form1 : 조회 진행
  end
end
@enduml
```

---

<a id="uc-ops-003"></a>
## UC-OPS-003 조회 상태 메시지/진행 인디케이터 갱신

### 1) 클라이언트 기준 상세 설명

- 사용자는 상태바를 통해 조회 시작/진행/완료/실패를 확인한다.
- 시스템은 단계별로 `SetStatusMessage`와 진행 인디케이터를 갱신한다.
- 완료 기준: 사용자가 현재 조회 상태를 상태바에서 즉시 구분할 수 있다.

### 2) 데이터 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-OPS-003 Data Flow

actor User
participant Form1
participant "StatusStrip" as StatusBar

User -> Form1 : 조회 동작 수행
Form1 -> StatusBar : 조회 중 메시지 + busy ON
Form1 -> StatusBar : 정리 중 메시지
Form1 -> StatusBar : 완료 또는 실패 메시지 + busy OFF
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-OPS-003 Logic Flow

actor User
participant Form1

User -> Form1 : 조회 시작
Form1 -> Form1 : SetStatusMessage(..., true)
alt 성공
  Form1 -> Form1 : SetStatusMessage(완료 메시지, false)
else 실패
  Form1 -> Form1 : SetStatusMessage(실패 메시지, false)
end
@enduml
```

### 3) 로직 흐름 (Sequence Diagram)

```plantuml
@startuml
title UC-CTV-002 Logic Flow

actor User
participant Form1

User -> Form1 : CCTV 카드 클릭
Form1 -> Form1 : 선택 카드 하이라이트
Form1 -> Form1 : 지도 마커 포커스 시도
Form1 -> Form1 : TryValidateCctvUrl
alt URL 검증 실패
  Form1 --> User : 오류 상태 메시지
else URL 정상
  alt 팝업 이미 열림
    Form1 --> User : 중복 열기 방지 메시지
  else
    Form1 -> Form1 : 팝업 열기 플래그 ON
    Form1 -> Form1 : CctvPlayerPopupForm 표시
    Form1 -> Form1 : 팝업 종료 후 플래그 OFF
    Form1 --> User : 종료 상태 메시지
  end
end
@enduml
```
