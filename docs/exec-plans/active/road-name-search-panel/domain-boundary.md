# Properties

status: verified

last_verified: 2026-04-08

# Domain Boundary

## Purpose

This plan stays within the existing traffic/map interaction domain. The work adds a road-name search entry point and routes the result into the mode already chosen by the user.

## In Scope

- Search input in the center panel above the map
- Road-name resolution to a highway candidate
- Dispatch into the existing traffic or CCTV flow based on the active mode
- Unit-level policy and routing coverage

## Out of Scope

- Redesign of the main form layout
- New map providers or tile server changes
- Public API key changes
- Unrelated VDS/CCTV data source behavior

## Boundary Notes

- The road-name search should respect the existing South Korea spatial policy.
- The work must preserve the current three-pane UI structure.
- The implementation should reuse the existing traffic and CCTV rendering paths rather than introducing a separate search mode.

