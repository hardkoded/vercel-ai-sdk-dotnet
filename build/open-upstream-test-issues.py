#!/usr/bin/env python3
"""Open the feature issues listed in tests/parity/generated/issues.json.

Uses the GitHub CLI (`gh`) against hardkoded/vercel-ai-sdk-dotnet. Issues that
already have a url are left alone. Bodies come from the same generator as
`parity-report.py`.
"""

from __future__ import annotations

import importlib.util
import json
import subprocess
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

spec = importlib.util.spec_from_file_location("parity_report", ROOT / "build" / "parity-report.py")
assert spec and spec.loader
parity = importlib.util.module_from_spec(spec)
spec.loader.exec_module(parity)

REPO = "hardkoded/vercel-ai-sdk-dotnet"
ISSUES = ROOT / "tests" / "parity" / "generated" / "issues.json"


def create_issue(title: str, body: str) -> str:
    delay = 2
    last_error = "gh issue create failed"
    for _ in range(6):
        result = subprocess.run(
            ["gh", "issue", "create", "--repo", REPO, "--title", title, "--body", body],
            check=False,
            capture_output=True,
            text=True,
        )
        if result.returncode == 0:
            url = result.stdout.strip().splitlines()[-1]
            if not url.startswith("https://"):
                raise SystemExit("Unexpected gh output: " + result.stdout)
            return url
        last_error = result.stderr.strip() or result.stdout.strip() or last_error
        if "rate limit" not in last_error.lower() and "abuse" not in last_error.lower():
            raise SystemExit(last_error)
        time.sleep(delay)
        delay = min(delay * 2, 60)
    raise SystemExit(last_error)


def existing_issue_urls() -> dict[str, str]:
    result = subprocess.run(
        ["gh", "issue", "list", "--repo", REPO, "--state", "all", "--limit", "2000", "--json", "title,url"],
        check=True,
        capture_output=True,
        text=True,
    )
    return {item["title"]: item["url"] for item in json.loads(result.stdout)}


def main() -> None:
    rows, manifest = parity.load_catalog()
    links = parity.load_links()
    commit = str(manifest["upstreamCommit"])
    if not ISSUES.exists():
        raise SystemExit(f"{ISSUES} is missing. Run build/parity-report.py first.")
    index = json.loads(ISSUES.read_text(encoding="utf-8"))
    features = {item["feature"]: item for item in parity.summarize(rows, links, commit)["features"]}
    existing = existing_issue_urls()
    opened = 0
    for issue in index["issues"]:
        if issue.get("url"):
            continue
        if issue["title"] in existing:
            issue["url"] = existing[issue["title"]]
            continue
        feature = features[issue["feature"]]
        body = parity.issue_body(feature, rows, links, commit)
        issue["url"] = create_issue(issue["title"], body)
        opened += 1
        ISSUES.write_text(json.dumps(index, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        print(issue["url"])
    print(f"opened {opened} issues")


if __name__ == "__main__":
    main()
