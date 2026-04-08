# Properties

status: verified

last_verified: 2026-04-08

# Detailed Design

## UI

- Add a compact search strip above the existing `WebView2` map inside the center panel.
- Keep the existing left controls, center map, and right result panel layout intact.
- The search strip should expose an input box and submit action for road-name lookup.
- The implementation inserts the strip from `Form1.cs` and leaves `Form1.Designer.cs` untouched so the layout change stays localized.

## Ports and Adapters

- Add `IRoadNameSearchPort` and a lightweight adapter over the existing OpenStreet data source.
- Use a minimal deterministic candidate selector over the OpenStreet results so the same query resolves consistently.
- Reuse the current traffic and CCTV service paths for the final mode-specific behavior.
- Keep adapters focused on data retrieval or resolution, not on UI orchestration.

## Service Responsibilities

- Resolve a road name to a highway candidate.
- Inspect the active mode.
- Dispatch into the current traffic or CCTV flow.
- Preserve request-version and status-message behavior already used by map clicks.
- Avoid introducing a separate search-result mode or a second results pane.

## Suggested Interface Shape

- `IRoadNameSearchPort`
  - `RoadNameCandidate Resolve(string roadName, MapBounds bounds)`
- `RoadNameSearchService`
  - `SearchRoadByName(string roadName, MapBounds bounds, CurrentMode mode)`

## DTOs / Value Objects

- `RoadNameSearchCommand`
- `RoadNameCandidate`
- `RoadSearchDispatchResult`
- `CurrentMode`
- `MapBounds`
- `RoadNameResolutionSelector`

## Test Points

- Road-name resolution returns a deterministic candidate for the same input.
- Active traffic mode routes to the traffic flow.
- Active CCTV mode routes to the CCTV flow.
- The search strip does not alter the existing three-pane layout.

## Implementation Notes

- The current mode should continue to be sourced from the existing left-side controls.
- The road-name search should use the same South Korea bounds policy already applied elsewhere.
- The implementation should avoid creating a separate search-mode result UI.
