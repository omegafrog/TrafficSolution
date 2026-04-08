# Properties

owner: Codex

status: completed

stop_state_at: 2026-04-02

title: 도로명 검색 기능 Use Cases

parent_docs:
- ../plan.md
- ../../../../usecase-spec.md

# Use Cases

이 문서는 2026-04-02 stop-state 기준 유스케이스 기록이다. `python3 .agents/skills/docs-verify/scripts/run.py`는 실행되어 통과했고, `dotnet build TrafficSolution.slnx`와 `dotnet test TrafficSolution.slnx`는 `NETSDK1100`으로 실패했다.

## 액터와 액션

- 액터: 사용자
- 액션:
  - 좌측 검색창에 도로명을 입력한다.
  - 검색을 실행한다.
  - 현재 우측 패널 모드에 맞는 결과를 확인한다.
  - 검색 결과가 없을 때 상태 메시지를 확인한다.

## UC-SCH-001 도로명 검색 기반 조회

- Related IDs: `UC-SCH-001`, `UC-TRF-001`, `UC-CTV-001`
- Goal: 현재 지도 bounds 내부 도로명 검색을 기존 클릭 downstream과 동일한 결과 화면으로 연결한다.

### 사전 조건

- WebView2 지도가 로딩 완료 상태다.
- `getMapViewState()`로 현재 지도 center/bounds를 읽을 수 있다.
- 우측 패널 모드는 `혼잡도` 또는 `CCTV` 중 하나로 선택되어 있다.

### 성공 시나리오

1. 사용자가 좌측 검색창에 도로명을 입력하고 실행한다.
2. 시스템이 현재 지도 center/bounds를 읽어 `SearchRoadByNameCommand`를 만든다.
3. 시스템이 검색어를 검증하고 bounds를 정규화한다.
4. 시스템이 query expander를 호출해 검색어 후보를 만든다.
5. 시스템이 현재 bounds 내부에서 `정확히 일치 > like 부분 일치` 순서로 도로를 검색한다.
6. 시스템이 현재 우측 패널 모드를 확인한다.
7. 혼잡도 모드면 매칭된 고속도로 집합 전체로 기존 traffic downstream을 실행한다.
8. CCTV 모드면 최상위 매칭 1건으로 기존 CCTV downstream을 실행한다.
9. 시스템이 기존 클릭 결과와 동일한 카드/마커/세그먼트 또는 CCTV 카드/마커를 표시한다.

### 대체 시나리오

- 검색어가 비어 있거나 공백뿐이면 조회를 시작하지 않는다.
- exact match가 있으면 partial 결과는 버린다.
- 검색 결과가 없으면 이전 결과를 유지하지 않고 현재 결과 컨텍스트를 비운 뒤 상태 메시지를 표시한다.

### 완료 기준

- 현재 map bounds 내부 도로만 검색된다.
- exact match가 있으면 exact 결과만 사용한다.
- exact가 없을 때만 partial 결과를 사용한다.
- 혼잡도/CCTV 결과 화면은 기존 지도 클릭 후속 동작과 동일한 렌더링 경로를 재사용한다.
- 클릭과 검색 모두 공통 downstream 메서드와 공통 UI wrapper를 사용한다.
- 테스트는 `TestProject1`에만 추가된다.
