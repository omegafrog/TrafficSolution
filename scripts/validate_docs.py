from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"

ALLOWED_STATUS = {"draft", "verified", "deprecated"}
LINK_RE = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")


def fail(msg: str) -> None:
    print(f"ERROR: {msg}")


def require_path(path: Path, errors: list[str]) -> None:
    if not path.exists():
        errors.append(f"missing required path: {path.relative_to(ROOT)}")


def check_plan(plan_path: Path) -> list[str]:
    errors: list[str] = []
    text = plan_path.read_text(encoding="utf-8")

    if "# Properties" not in text:
        errors.append(f"{plan_path}: missing '# Properties'")

    for field in ["owner:", "status:", "title:", "parent_docs:"]:
        if field not in text:
            errors.append(f"{plan_path}: missing '{field}'")

    match = re.search(r"status:\s*([A-Za-z0-9_-]+)", text)
    if not match:
        errors.append(f"{plan_path}: status value not found")
    elif match.group(1) not in ALLOWED_STATUS:
        errors.append(f"{plan_path}: invalid status '{match.group(1)}'")

    for _, raw_target in LINK_RE.findall(text):
        if raw_target.startswith("http://") or raw_target.startswith("https://"):
            continue
        target = (plan_path.parent / raw_target).resolve()
        if not target.exists():
            errors.append(f"{plan_path}: broken link -> {raw_target}")

    for required_name in [
        "domain-boundary.md",
        "use-cases.md",
        "event-storming.md",
        "detailed-design.md",
    ]:
        if required_name not in text:
            errors.append(f"{plan_path}: missing reference to {required_name}")

    return errors


def main() -> int:
    errors: list[str] = []

    for required in [
        DOCS / "design-docs" / "index.md",
        DOCS / "product-specs" / "index.md",
        DOCS / "exec-plans" / "index.md",
        DOCS / "exec-plans" / "active" / "index.md",
        DOCS / "exec-plans" / "completed" / "index.md",
        DOCS / "references" / "README.md",
    ]:
        require_path(required, errors)

    plan_files = list(DOCS.glob("exec-plans/active/**/plan.md"))
    for plan in plan_files:
        errors.extend(check_plan(plan))

    if errors:
        for error in errors:
            fail(error)
        print(f"\nFAILED: {len(errors)} issue(s)")
        return 1

    if not plan_files:
        print("OK: no active plans yet")
        return 0

    print("OK: documentation structure validated")
    return 0


if __name__ == "__main__":
    sys.exit(main())

