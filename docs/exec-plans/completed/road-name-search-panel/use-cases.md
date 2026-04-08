# Properties

status: verified

last_verified: 2026-04-08

# Use Cases

## UC-RSN-001 Road-name search executes in the active mode

- Actor: user
- Action: enter a road name and submit the search
- Intent: find the road and run the same behavior the app already uses for the current mode
- Success scenario:
  1. User enters a road name in the search panel above the map.
  2. System resolves the road name to a highway candidate.
  3. System inspects the currently active mode.
  4. System runs the traffic flow when traffic mode is active.
  5. System runs the CCTV flow when CCTV mode is active.
- Completion criteria:
  - The search result is visible through the existing traffic or CCTV UI path.
  - No separate search-result mode is introduced.

## UC-RSN-002 Road-name resolution remains deterministic

- Actor: system
- Action: choose one candidate when road names are ambiguous
- Intent: avoid unstable or surprising routing
- Success scenario:
  1. Multiple road candidates match the entered text.
  2. System applies a deterministic tie-breaker.
  3. System forwards the selected highway candidate into the active mode flow.
- Completion criteria:
  - The same input resolves to the same candidate under the same data set and bounds.

## UC-RSN-003 Search panel stays within the center pane

- Actor: user
- Action: use the new road-name panel
- Intent: keep the existing layout while adding search entry
- Success scenario:
  1. System renders a small search strip above the map.
  2. Map, left controls, and right results remain in the current layout.
- Completion criteria:
  - The three-pane structure is preserved.

