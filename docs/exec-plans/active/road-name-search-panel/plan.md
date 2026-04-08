# Properties

title: Road-name Search Panel
owner: Codex
status: verified
parent_docs:
- [README.md](../../../../README.md)
- [docs/usecase-spec.md](../../../../docs/usecase-spec.md)
domain: Traffic search / map interaction
last_verified: 2026-04-08

# Task Summary

Implemented a road-name search input above the map and routed searches into the app’s current mode-specific flow.

# Domain Boundary

- Source: [`domain-boundary.md`](domain-boundary.md)
- Scope: road-name search entry, road resolution, and mode-aligned routing in the map-centered UI.

# Use Cases

- Source: [`use-cases.md`](use-cases.md)
- Primary use case: search by road name and execute the traffic or CCTV path that matches the active mode.

# Event Storming

- Source: [`event-storming.md`](event-storming.md)
- Capture command, event, and policy boundaries for road-name resolution and mode dispatch.

# Detailed Design

- Source: [`detailed-design.md`](detailed-design.md)
- Capture ports, adapters, DTOs, and test points needed for implementation.

# Implementation Plan

- [x] Add a search bar container above the WebView2 map in the center panel.
- [x] Add a road-name resolution path that maps user input to a highway candidate within the existing spatial policy.
- [x] Route the resolved result into the already-selected traffic or CCTV flow without creating a separate result mode.
- [x] Keep current request-version guards and status messages aligned with the existing click-driven flows.
- [x] Add unit tests in `TestProject1` for road-name resolution and mode-aware routing.

# Verification Plan

- `python3 scripts/validate_docs.py`
- `dotnet build TrafficSolution.slnx`
- `dotnet test TrafficSolution.slnx`
- Result in this worktree:
  - `python3 scripts/validate_docs.py` passed.
  - `dotnet build TrafficSolution.slnx -p:EnableWindowsTargeting=true` passed.
  - `dotnet test TrafficSolution.slnx -p:EnableWindowsTargeting=true` could not complete in this Linux environment because the testhost requires `Microsoft.WindowsDesktop.App 10.0.0`, which is not installed here.
- Fallback if routing or resolution proves ambiguous: refine the lookup tie-breaker in the design docs before implementation.

# Documentation Plan

- Update the active planning package under `docs/exec-plans/active/road-name-search-panel/`.
- Keep `docs/exec-plans/active/index.md` linked to this plan.
- Preserve the relationship to `README.md` and `docs/usecase-spec.md`.

# Output Files

- `docs/exec-plans/active/road-name-search-panel/plan.md`
- `docs/exec-plans/active/road-name-search-panel/domain-boundary.md`
- `docs/exec-plans/active/road-name-search-panel/use-cases.md`
- `docs/exec-plans/active/road-name-search-panel/event-storming.md`
- `docs/exec-plans/active/road-name-search-panel/detailed-design.md`

# Execution Result

- Implemented in the worktree only.
- Changed files:
  - [TrafficForm/Adapter/OpenStreetDbRepository.cs](../../../../TrafficForm/Adapter/OpenStreetDbRepository.cs)
  - [TrafficForm/Adapter/RoadNameSearchAdapter.cs](../../../../TrafficForm/Adapter/RoadNameSearchAdapter.cs)
  - [TrafficForm/App/CurrentMode.cs](../../../../TrafficForm/App/CurrentMode.cs)
  - [TrafficForm/App/RoadNameSearchCommand.cs](../../../../TrafficForm/App/RoadNameSearchCommand.cs)
  - [TrafficForm/App/RoadNameSearchService.cs](../../../../TrafficForm/App/RoadNameSearchService.cs)
  - [TrafficForm/App/RoadSearchDispatchResult.cs](../../../../TrafficForm/App/RoadSearchDispatchResult.cs)
  - [TrafficForm/Domain/MapBounds.cs](../../../../TrafficForm/Domain/MapBounds.cs)
  - [TrafficForm/Domain/RoadNameCandidate.cs](../../../../TrafficForm/Domain/RoadNameCandidate.cs)
  - [TrafficForm/Domain/RoadNameResolutionSelector.cs](../../../../TrafficForm/Domain/RoadNameResolutionSelector.cs)
  - [TrafficForm/Port/IRoadNameSearchPort.cs](../../../../TrafficForm/Port/IRoadNameSearchPort.cs)
  - [TrafficForm/Program.cs](../../../../TrafficForm/Program.cs)
  - [TrafficForm/UI/Form1.Cctv.cs](../../../../TrafficForm/UI/Form1.Cctv.cs)
  - [TrafficForm/UI/Form1.cs](../../../../TrafficForm/UI/Form1.cs)
  - [TestProject1/RoadNameResolutionSelectorTest.cs](../../../../TestProject1/RoadNameResolutionSelectorTest.cs)
  - [TestProject1/RoadNameSearchServiceTest.cs](../../../../TestProject1/RoadNameSearchServiceTest.cs)
- The search strip was inserted from `Form1.cs`, and `Form1.Designer.cs` was left untouched so the layout change stays localized.
- Road-name resolution uses a minimal deterministic path over the OpenStreet data source.
- Search submission is routed into the existing traffic or CCTV flows based on the active left-side mode, without creating a separate search-result mode.

# Assumption if present in the oracle output

- “Current mode” means the active mode already selected in the existing left-side controls.
- The search panel belongs above the map in the center pane, not in the right results pane.

# Out of Scope if present in the oracle output

- No redesign of the overall main form layout.
- No new map provider, tile server, or public API key changes.
- No unrelated VDS/CCTV data source changes beyond what is needed for road-name routing.
