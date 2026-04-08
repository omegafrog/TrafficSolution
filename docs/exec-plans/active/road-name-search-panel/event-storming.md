# Properties

status: verified

last_verified: 2026-04-08

# Event Storming

## Commands

- `SubmitRoadNameSearch` for UC-RSN-001
- `ResolveRoadNameCandidate` for UC-RSN-002
- `DispatchResolvedRoadSearch` for UC-RSN-001

## Events

- `RoadNameSearchSubmitted` for UC-RSN-001
- `RoadNameCandidateResolved` for UC-RSN-002
- `RoadSearchRoutedToTrafficMode` for UC-RSN-001
- `RoadSearchRoutedToCctvMode` for UC-RSN-001

## Policies

- `RoadNameSearchUsesActiveModePolicy` for UC-RSN-001
- `RoadNameResolutionIsDeterministicPolicy` for UC-RSN-002
- `RoadNameSearchStaysWithinKoreaBoundsPolicy` for UC-RSN-001 and UC-RSN-002

## Read Models

- `RoadNameSearchStateReadModel`
- `RoadNameResolutionResultReadModel`
- `ActiveModeSearchDispatchReadModel`

## Aggregate

- `RoadNameSearchRequestAggregate`

## Traceability Notes

- The routing events and policies must remain tied to the active-mode road-name search use case.
- The deterministic resolution policy must remain tied to the ambiguous-match use case.

