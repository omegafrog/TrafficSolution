# Favorites 기능 설계서 (Use Case + Event Storming)

## 1) 설계 배경

- 사용자 요구: 자주 사용하는 고속도로/좌표를 즐겨찾기로 저장하고 재사용한다.
- 저장 대상
  - 고속도로 즐겨찾기: `고속도로 번호 + 기준 좌표 + 뷰(min/max lat/lon) + zoom`
  - 좌표 즐겨찾기: `사용자 지정 이름 + 기준 좌표 + 뷰(min/max lat/lon) + zoom`
- UI 요구
  - 상단 toolbox에 `고속도로 즐겨찾기`/`좌표 즐겨찾기` 탭 제공
  - 탭 클릭 시 우측 패널 콘텐츠를 즐겨찾기 내용으로 전환
  - 고속도로 즐겨찾기는 고속도로 번호 기준 트리(부모=고속도로 번호, 자식=복수 즐겨찾기)
- 우측 패널 열기/닫기 버튼 제공
- split panel 수동 크기 변경 잠금
- **우측 패널 너비 동적 조정**
  - VDS/CTV 조회 결과: 320px (축소)
  - 즐겨찾기 모드: 520px (원래 너비)
  - 조회 결과 탭 클릭 시에도 320px 적용
  - split panel 수동 크기 변경 잠금

---

## 2) 도메인 분류

- 도메인: `Favorites`
- 핵심 엔티티
  - `MapViewSnapshot`
  - `HighwayFavorite`
  - `CoordinateFavorite`
  - `UserFavorites`

---

## 3) 액터/액션

- 액터: 사용자
- 액션
  1. 현재 고속도로 조회 컨텍스트를 고속도로 즐겨찾기로 저장
  2. 고속도로 즐겨찾기 트리에서 항목 선택/적용
  3. 현재 지도 뷰를 이름 있는 좌표 즐겨찾기로 저장
  4. 좌표 즐겨찾기 목록에서 항목 선택/적용
  5. 우측 패널 열기/닫기

---

## 4) 유스케이스

### UC-FAV-001 고속도로 즐겨찾기 저장
- 사전 조건: 최소 1회 혼잡도 조회 결과가 존재한다.
- 성공 시나리오
  1. 사용자가 `고속도로 즐겨찾기` 탭으로 이동한다.
  2. 사용자가 `현재 고속도로 저장`을 클릭한다.
  3. 시스템이 현재 고속도로 번호, 기준 좌표, 현재 뷰(bounds/zoom)를 수집한다.
  4. 시스템이 즐겨찾기 저장소에 항목을 append한다.
  5. 시스템이 트리를 고속도로 번호 기준으로 다시 렌더링한다.
- 완료 기준: 트리에 부모(고속도로 번호) 아래 자식 즐겨찾기가 생성된다.

### UC-FAV-002 고속도로 즐겨찾기 적용
- 사전 조건: 저장된 고속도로 즐겨찾기가 1개 이상 존재한다.
- 성공 시나리오
  1. 사용자가 트리의 하위 즐겨찾기를 더블클릭한다.
  2. 시스템이 저장된 좌표/뷰(bounds/zoom)로 지도 이동을 수행한다.
  3. 시스템이 해당 고속도로 번호 우선으로 혼잡도 조회를 수행한다.
  4. 시스템이 우측 패널을 조회 결과 모드로 전환하고 카드/마커를 갱신한다.
- 완료 기준: 저장된 컨텍스트에 맞는 지도/우측 리스트가 복원된다.

### UC-FAV-003 좌표 즐겨찾기 저장
- 사전 조건: 지도(WebView2)가 로딩 완료 상태다.
- 성공 시나리오
  1. 사용자가 `좌표 즐겨찾기` 탭으로 이동한다.
  2. 사용자가 `현재 좌표 저장` 클릭 후 이름을 입력한다.
  3. 시스템이 현재 중심 좌표 + bounds + zoom을 수집한다.
  4. 시스템이 저장소에 좌표 즐겨찾기를 append한다.
  5. 시스템이 목록을 이름 기준으로 다시 렌더링한다.
- 완료 기준: 사용자 지정 이름으로 좌표 즐겨찾기 항목이 생성된다.

### UC-FAV-004 좌표 즐겨찾기 적용
- 사전 조건: 저장된 좌표 즐겨찾기가 1개 이상 존재한다.
- 성공 시나리오
  1. 사용자가 좌표 즐겨찾기 항목을 더블클릭한다.
  2. 시스템이 저장된 bounds/zoom으로 지도 뷰를 복원한다.
- 완료 기준: 지도 뷰가 저장 시점 상태로 이동한다.

### UC-UI-001 우측 패널 열기/닫기
- 성공 시나리오
  1. 사용자가 상단 toolbox의 우측 패널 토글 버튼을 클릭한다.
  2. 시스템이 `Panel2Collapsed`를 토글한다.
  3. 시스템이 버튼 텍스트를 현재 상태에 맞게 갱신한다.
- 완료 기준: 우측 패널 상태와 버튼 텍스트가 일치한다.

---

## 5) 이벤트 스토밍 결과

### 5.1 Command
- `SaveHighwayFavorite`
- `ApplyHighwayFavorite`
- `SaveCoordinateFavorite`
- `ApplyCoordinateFavorite`
- `RemoveHighwayFavorite`
- `RemoveCoordinateFavorite`
- `ToggleRightPanel`

### 5.2 Domain Event
- `HighwayFavoriteSaved`
- `HighwayFavoriteApplied`
- `CoordinateFavoriteSaved`
- `CoordinateFavoriteApplied`
- `FavoriteRemoved`
- `RightPanelToggled`

### 5.3 Policy
- 동일 고속도로 번호는 부모 노드 1개로 그룹화하고, 하위 즐겨찾기 다중 허용
- 좌표 즐겨찾기 이름은 공백 불가
- 고속도로 즐겨찾기 저장 시 고속도로 번호가 유효(1 이상)해야 함
- 우측 패널 split은 사용자 드래그로는 변경 불가(고정)

### 5.4 Read Model
- `HighwayFavoritesTreeReadModel` (고속도로 번호 정렬 + 자식 목록)
- `CoordinateFavoritesListReadModel` (이름/좌표/zoom/저장시각)
- `RightPanelStateReadModel` (열림/닫힘)

### 5.5 Aggregate
- `UserFavorites` Aggregate
  - HighwayFavorites
  - CoordinateFavorites

---

## 6) 코드 반영 매핑

- Domain
  - `TrafficForm/Domain/MapViewSnapshot.cs`
  - `TrafficForm/Domain/HighwayFavorite.cs`
  - `TrafficForm/Domain/CoordinateFavorite.cs`
  - `TrafficForm/Domain/UserFavorites.cs`
- Port/Adapter
  - `TrafficForm/Port/IFavoriteStorePort.cs`
  - `TrafficForm/Adapter/JsonFavoriteStoreAdapter.cs`
- Service
  - `TrafficForm/App/FavoriteService.cs`
- UI
  - `TrafficForm/UI/Form1.Favorites.cs`
  - `TrafficForm/UI/Form1.cs`
  - `TrafficForm/UI/Form1.Cctv.cs`

---

## 7) 후속 개선 포인트

- 즐겨찾기 이름 편집(rename) 기능
- 고속도로 즐겨찾기 저장 시 사용자 지정 이름 입력 옵션
- 즐겨찾기 export/import
- 즐겨찾기 항목별 마지막 사용 시각 기반 정렬 옵션
