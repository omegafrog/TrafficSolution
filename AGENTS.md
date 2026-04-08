# Project Overview

TrafficSolution is a .NET 10 C# WinForms application that uses public traffic data and a local OpenStreetMap tile server to show highway congestion, VDS traffic data, and CCTV information.

The main form is organized into three split panels:

- Left: filters and mode controls
- Center: WebView2 map backed by the local tile server
- Right: hidden by default, shows VDS and CCTV result cards when a highway is selected

# Build/Test Commands

- `dotnet build TrafficSolution.slnx`: builds the solution.
- `dotnet test TrafficSolution.slnx`: runs the full unit test suite.
- When creating a new git worktree, place it under the parent directory's `worktrees/` folder, for example `../worktrees/<name>`.

If these commands become inaccurate, update this file instead of guessing.

# Documentation Navigation

- [docs/design-docs](docs/design-docs/index.md): design decisions, architecture notes, and design verification artifacts.
- [docs/product-specs](docs/product-specs/index.md): product requirements, functional requirements, and use cases.
- [docs/exec-plans/active](docs/exec-plans/active/index.md): active execution plans for `orchestrate-plan`.
- [docs/exec-plans/completed](docs/exec-plans/completed/index.md): archived execution plans.
- `docs/generated`: generated artifacts that should remain in the repository.
- [docs/references](docs/references/README.md): supporting references for agents and developers.

# Documentation Policy

- `docs/` is the source of truth.
- Planning work for `orchestrate-plan` must be created under `docs/`.
- Any planning docs under `docs/` must be validated with `python3 scripts/validate_docs.py`.

# Constraints

- Final and external coordinates must use `EPSG:4326`; internal spatial math may use `EPSG:3857`, but must convert back before returning values.
- Restrict map/search/query coordinates to South Korea bounds only: `MIN_LATITUDE = 33.0`, `MAX_LATITUDE = 39.0`, `MIN_LONGITUDE = 125.0`, `MAX_LONGITUDE = 132.0`.
- When a map click is used for lookup, pass the current view bounds and filter queries by min/max latitude and longitude.
- Public API keys must come from environment variables only. Do not commit real keys or put them in docs or source.
- Use `SERVICE_KEY` for VDS public data and `CCTV_SERVICE_KEY` for CCTV public data.
- Treat `Form` code as UI only. `Program.cs` is the composition root. Services depend on ports, adapters implement ports, and domain logic belongs in `Domain`.
- Keep dependency injection constructor-based and store external dependencies in fields.
- Write new or changed tests only in `TestProject1`, and keep them focused on unit-level policy/contract behavior.
- Do not use `NotImplementedException` as an operational failure path.
- Custom exceptions must carry both `ExceptionCode` and `Description`.
- After code changes, run build first and then the full unit test suite. Do not launch the app after build.

# Validation

After creating or updating planning docs under `docs/`, run:

```bash
python3 scripts/validate_docs.py
```

If validation fails:

- do not ignore failures
- fix broken links, missing required sections, or invalid properties
- rerun the command until it passes

