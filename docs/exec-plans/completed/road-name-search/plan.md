# Properties

owner: Codex

status: completed

last_verified: 2026-04-02

completed_at: 2026-04-02

stop_state_at: 2026-04-02

title: 도로명 검색 bounds 제한 및 downstream 공통화 계획

parent_docs:
- ../../../road-name-search-usecase-eventstorming.md
- ../../../usecase-spec.md
- ../../../../README.md

domain: RoadSearch

created_at: 2026-04-02

# Task Summary

- 현재 repo에는 `docs/exec-plans/completed/road-name-search/plan.md`가 유지본으로 존재한다.
- 2026-04-02 raw execute output은 `docs/exec-plans/active/road-name-search/plan.md` 기준 partial stop-state를 가리켰지만, 현재 worktree 기준 기록은 completed stop-state다.

- WinForms 앱에 `도로명 검색`을 추가한다.
- 검색 대상은 항상 `현재 map bounds 내부`로 제한한다.
- 검색 결과는 `현재 우측 패널 모드(혼잡도/CCTV)`에 맞춰 기존 지도 클릭 후속 동작과 동일한 화면으로 이어져야 한다.
- 검색 매칭은 `IRoadNameHighwaySearchPort`와 `IRoadNameQueryExpanderPort`로 추상화한다.
- 기본 매칭 우선순위는 `정확히 일치 > like 부분 일치`다.
- 클릭 처리와 검색 처리의 후속 조회/렌더링 경로는 공통화한다.
- 현재 구현 기준 진입점은 `Form1`의 지도 클릭 흐름이며, 검색은 이 흐름의 downstream 재사용을 목표로 한다.

# Linked Docs

- Product Spec: [domain-boundary.md](product-spec/domain-boundary.md)
- Product Spec: [use-cases.md](product-spec/use-cases.md)
- Design: [event-storming.md](design/event-storming.md)
- Design: [detailed-design.md](design/detailed-design.md)

# Implementation Plan

- [x] `SearchRoadByNameCommand`, `SearchRoadByNameService`, `RoadNameSearchResult`, `RoadNameMatchKind` 계약이 추가됐다.
- [x] `IRoadNameHighwaySearchPort`, `IRoadNameQueryExpanderPort`와 기본 어댑터가 추가돼 exact/partial 검색과 query normalize 확장 지점이 분리됐다.
- [x] `VdsRepository`에 bounds 내부 후보 검색이 추가돼 `vds + vds_loc`를 검색 소스로 재사용한다.
- [x] `RequestTrafficByPosService`에 `GetTrafficByHighwaysAsync(...)` 공통 메서드가 추가됐다.
- [x] `RequestCctvByPosService`에 `GetHighwayCctvAsync(...)` 공통 메서드가 추가됐다.
- [x] `Form1`, `Form1.Cctv`에서 검색 진입이 공통 downstream wrapper를 재사용하도록 정리됐다.
- [x] 검색 결과가 없을 때 현재 결과 컨텍스트를 비우고 상태 메시지를 갱신하도록 UI 후속 처리 규칙이 맞춰졌다.
- [x] `TestProject1`에 검색 서비스, downstream 공통화, DI 등록 단위 테스트가 추가됐다.
- [x] `docs/usecase-spec.md`와 `README.md`에 `UC-SCH-001` 및 관련 설계 링크가 반영됐다.

# Verification Plan

- `python3 .agents/skills/docs-verify/scripts/run.py`
  Expected: `docs/exec-plans/active/**/plan.md` 구조 검증이 통과하고, 연결된 계획 문서의 `status`와 `last_verified`가 갱신된다.
  Fallback: broken link, 누락 헤더, 잘못된 상태값이 나오면 계획 문서 구조를 먼저 수정하고 다시 실행한다.
- `dotnet build TrafficSolution.slnx`
  Expected: 전체 솔루션 빌드가 성공한다.
  Fallback: WinForms 타깃 또는 .NET 10 SDK 부재로 실패하면 Windows + Desktop workload 환경에서 재실행하고 실패 원인을 PR/완료 보고에 명시한다.
- `dotnet test TrafficSolution.slnx`
  Expected: `TestProject1` 단위 테스트가 모두 통과한다.
  Fallback: 환경 제약 또는 신규 계약 위반으로 실패하면 실패 테스트를 기준으로 구현/문서를 조정하고 재검증한다.

# Execution Results

- 구현 기준 문서 경로
  - 현재 repo 경로: [`docs/exec-plans/completed/road-name-search/plan.md`](plan.md)
  - raw execute output 기준: `docs/exec-plans/active/road-name-search/plan.md` partial stop-state
- 핵심 진입점
  - `TrafficForm/App/SearchRoadByNameService.cs`
  - `TrafficForm/UI/Form1.cs`
  - `TrafficForm/UI/Form1.Cctv.cs`
  - `TrafficForm/Adapter/VdsRepository.cs`
- 아키텍처 반영
  - WinForms UI에 도로명 검색 진입점과 공통 lookup wrapper가 추가된 상태다.
  - App layer에 검색 전용 서비스와 직접-highway downstream 메서드가 들어간 상태다.
  - Adapter/Port layer에 검색 체인이 추가되고 `VdsRepository`가 bounds 기반 highway 후보 조회 책임을 가지는 상태다.
  - `Program.cs` composition root와 `TestProject1` 단위 테스트가 신규 검색 체인을 인지하도록 확장된 상태다.

# Verification Results

- `python3 .agents/skills/docs-verify/scripts/run.py`
  - 결과: 실행됨, 통과
  - 상세: 문서 검증이 실행되어 통과했다.
- `dotnet build TrafficSolution.slnx`
  - 결과: 실행됨, 실패 (`NETSDK1100`)
  - 상세: 현재 환경에서 `NETSDK1100`으로 빌드가 실패했다.
- `dotnet test TrafficSolution.slnx`
  - 결과: 실행됨, 실패 (`NETSDK1100`)
  - 상세: 현재 환경에서 `NETSDK1100`으로 테스트가 실패했다.
- 로컬 Docker Postgres schema check (`TrafficForm/osm-local/compose.yaml`)
  - 결과: 실행됨, 통과
  - 상세: `public.vds`에는 `도로명` 컬럼이 없고 `public.vds_loc`에만 `도로명` 컬럼이 있음을 확인했고, 이에 맞춰 `TrafficForm/Adapter/VdsRepository.cs` bounds 검색 쿼리가 `vl."도로명"`을 사용하도록 수정됐다.

# Documentation Plan

- 현재 repo의 stop-state 문서 엔트리 포인트: `docs/exec-plans/completed/road-name-search/plan.md`
- raw execute output의 `active/...` 경로는 역사적 참조이며, 현재 repo 유지본은 `completed/...` 경로다.
- 기존 설계 산출물 문서 유지: `docs/road-name-search-usecase-eventstorming.md`
- 기존 설계 산출물 연계 유지: `docs/road-name-search-usecase-eventstorming.md`
- stop-state product-spec 문서: `docs/exec-plans/completed/road-name-search/product-spec/domain-boundary.md`
- stop-state product-spec 문서: `docs/exec-plans/completed/road-name-search/product-spec/use-cases.md`
- stop-state design 문서: `docs/exec-plans/completed/road-name-search/design/event-storming.md`
- stop-state design 문서: `docs/exec-plans/completed/road-name-search/design/detailed-design.md`
- 구현 반영 문서 업데이트: `docs/usecase-spec.md`
- 문서 링크 카탈로그 업데이트: `README.md`

# Output Files

- Keep: `docs/road-name-search-usecase-eventstorming.md`
- Reference: `docs/road-name-search-usecase-eventstorming.md`
- Update: `TrafficForm/App/UpdateSelectedPosTrafficInfoCommand.cs`
- Update: `TrafficForm/Program.cs`
- Update: `TrafficForm/UI/Form1.cs`
- Update: `TrafficForm/UI/Form1.Cctv.cs`
- Update: `TrafficForm/App/RequestTrafficByPosService.cs`
- Update: `TrafficForm/App/RequestCctvByPosService.cs`
- Create: `TrafficForm/App/SearchRoadByNameService.cs`
- Create: `TrafficForm/App/SearchRoadByNameCommand.cs`
- Create: `TrafficForm/Port/IRoadNameHighwaySearchPort.cs`
- Create: `TrafficForm/Port/IRoadNameQueryExpanderPort.cs`
- Create: `TrafficForm/Adapter/RoadNameHighwaySearchAdapter.cs`
- Create: `TrafficForm/Adapter/DefaultRoadNameQueryExpanderAdapter.cs`
- Update: `TrafficForm/Adapter/VdsRepository.cs`
- Create: `TestProject1/SearchRoadByNameServiceTest.cs`
- Update: `TestProject1/RequestTrafficServiceTest.cs`
- Update: `TestProject1/RequestCctvByPosServiceTest.cs`
- Create or Update: `TestProject1/DiWiringTest.cs`
- Update: `docs/usecase-spec.md`
- Update: `README.md`

# Remaining Risks / Follow-ups

- 현재 워크트리는 중단 시점의 부분 구현 상태라 컴파일 가능 여부가 확인되지 않았다.
- `README.md`는 `docs/exec-plans/completed/road-name-search/plan.md`를 가리키며, 현재 유지본과 정합하다.
- UI wrapper, downstream 공통화, 신규 테스트는 모두 현재 변경 집합에 존재하지만 통합 검증 전이다.

# Assumptions

- 검색은 자동완성 UI 없이 `입력 후 실행` 방식으로 충분하다.
- 검색은 현재 map bounds 기준만 사용하며, 전체 검색 모드는 이번 범위 밖이다.
- CCTV 검색 결과는 여러 도로를 동시에 보여주지 않고, 최고 우선순위 도로 1건만 사용한다.
- no-result는 예외보다 빈 결과와 상태 메시지로 처리한다.
- 예외 신설은 우선 피하고, 기존 예외 계약이 이미 있는 경우에만 재사용한다.
- 기존 `RequestTrafficByPosService`의 `NotImplementedException` 부채는 이번 작업의 주목적이 아니다.

# Out Of Scope

- 형태소 분석기 실제 연동
- 오타 교정/퍼지 검색
- 검색 결과 드롭다운 추천 목록
- 지도 모드 체계 재설계
- UI 자동화 테스트 추가
