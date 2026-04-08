# Properties

owner: Codex

status: draft

stop_state_at: 2026-04-02

title: 도로명 검색 기능 Event Storming

parent_docs:
- ../plan.md
- ../../../../road-name-search-usecase-eventstorming.md

# Event Storming

이 문서는 2026-04-02 partial stop-state 기준 이벤트 스토밍 기록이다. `python3 .agents/skills/docs-verify/scripts/run.py`는 실행되어 통과했고, `dotnet build TrafficSolution.slnx`와 `dotnet test TrafficSolution.slnx`는 `NETSDK1100`으로 실패했다.

oracle는 explicit event 이름을 제공하지 않았습니다. 아래 Event 섹션의 항목은 `UC-SCH-001` traceability를 유지하기 위한 editorial structure이며, 새로운 구현 범위를 추가하지 않습니다.

## Command

| Command | Source | Related Use Case | Intent |
|---|---|---|---|
| `SearchRoadByNameCommand` | oracle explicit | `UC-SCH-001` | 검색어와 현재 center/bounds를 앱 서비스로 전달한다. |

## Event

| Event | Source | Related Use Case | Intent |
|---|---|---|---|
| `RoadNameSearchRequested` | editorial-only trace | `UC-SCH-001` | 검색 요청이 validation 단계로 진입했음을 표현한다. |
| `RoadNameSearchMatched` | editorial-only trace | `UC-SCH-001` | bounds 내부에서 exact 또는 partial 후보가 선택됐음을 표현한다. |
| `RoadNameSearchMissed` | editorial-only trace | `UC-SCH-001` | bounds 내부에서 매칭 후보가 없음을 표현한다. |
| `TrafficLookupCompleted` | editorial-only trace | `UC-SCH-001` | traffic downstream 공통 경로 완료를 표현한다. |
| `CctvLookupCompleted` | editorial-only trace | `UC-SCH-001` | CCTV downstream 공통 경로 완료를 표현한다. |
| `CurrentLookupContextCleared` | editorial-only trace | `UC-SCH-001` | 검색 miss 시 현재 결과 컨텍스트 정리를 표현한다. |

## Policy

| Policy | Related Use Case | Source |
|---|---|---|
| 검색 대상은 항상 현재 map bounds 내부로 제한한다. | `UC-SCH-001` | oracle explicit |
| 검색 데이터 소스는 `planet_osm_line` 대신 `vds + vds_loc`를 우선 사용한다. | `UC-SCH-001` | oracle explicit |
| exact 결과가 1건 이상이면 partial은 반환하지 않는다. | `UC-SCH-001` | oracle explicit |
| 형태소 확장 지점은 `IRoadNameQueryExpanderPort` 교체만으로 확장 가능해야 한다. | `UC-SCH-001` | oracle explicit |
| 클릭과 검색은 후보 고속도로가 정해진 뒤부터 공통 downstream 경로를 사용한다. | `UC-SCH-001` | oracle explicit |
| 혼잡도 모드는 매칭된 고속도로 집합 전체를 대상으로 결과를 그린다. | `UC-SCH-001` | oracle explicit |
| CCTV 모드는 최상위 매칭 1건만 선택해 기존 단일 고속도로 흐름으로 보낸다. | `UC-SCH-001` | oracle explicit |
| no-result는 예외보다 빈 결과와 상태 메시지로 처리하고 현재 결과 컨텍스트를 비운다. | `UC-SCH-001` | oracle explicit |

## Read Model

| Read Model | Related Use Case | Fields |
|---|---|---|
| `RoadNameSearchResult` | `UC-SCH-001` | `Highways`, `MatchKind` |
| `CurrentMapViewState` | `UC-SCH-001` | `Latitude`, `Longitude`, `MinLongitude`, `MinLatitude`, `MaxLongitude`, `MaxLatitude` |
| `TrafficLookupResult` | `UC-SCH-001` | 기존 `Dictionary<int, List<VdsTrafficResult>>` downstream 결과 |
| `HighwayCctvSelection` | `UC-SCH-001` | `HighwayNo`, `HighwayName`, `CctvInfos` |

## Aggregate

- 신규 aggregate는 도입하지 않는다.
- 본 기능은 저장 상태를 추가하지 않는 조회성 유스케이스이며, `RoadNameSearchResult`와 기존 조회 read model을 조합해 흐름을 구성한다.
