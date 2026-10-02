#!/usr/bin/env python3
"""Rebuild tests/parity/coverage-summary.json from UpstreamTest attributes.

The summary is checked by UpstreamParityTests. Issue bodies for features that
still have missing or partial upstream tests are written as JSON lines when
--issues is set.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PARITY = ROOT / "tests" / "parity"
TEST_DIR = ROOT / "tests" / "Vercel.AI.Tests"

ATTR = re.compile(
    r'\[UpstreamTest\(\s*(?:(?P<prefix>[A-Za-z_][A-Za-z0-9_]*)\s*\+\s*)?"(?P<id>(?:\\.|[^"\\])*)"'
    r'(?P<rest>.*?)\)\]',
    re.S,
)
CONST_RE = re.compile(r'const\s+string\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*"((?:\\.|[^"\\])*)"')
CLASS_RE = re.compile(r"\bclass\s+(\w+)")
METHOD_RE = re.compile(r"public\s+(?:async\s+)?(?:Task|void)\s+(\w+)\s*\(")
COVERAGE_RE = re.compile(r"Coverage\s*=\s*UpstreamCoverage\.(Covered|Partial)")
NOTE_RE = re.compile(r'Note\s*=\s*"((?:\\.|[^"\\])*)"')


def unescape(value: str) -> str:
    return json.loads('"' + value + '"')


def load_links() -> list[dict[str, str]]:
    links: list[dict[str, str]] = []
    for path in sorted(TEST_DIR.rglob("*.cs")):
        text = path.read_text(encoding="utf-8")
        constants = {name: unescape(value) for name, value in CONST_RE.findall(text)}
        class_name = "Tests"
        cursor = 0
        for match in ATTR.finditer(text):
            class_match = None
            for candidate in CLASS_RE.finditer(text, cursor, match.start()):
                class_match = candidate
            if class_match is not None:
                class_name = class_match.group(1)
            method = METHOD_RE.search(text, match.end())
            if method is None:
                raise SystemExit(f"No test method after UpstreamTest in {path}")
            rest = match.group("rest")
            coverage = COVERAGE_RE.search(rest)
            note = NOTE_RE.search(rest)
            upstream_id = unescape(match.group("id"))
            prefix_name = match.group("prefix")
            if prefix_name:
                if prefix_name not in constants:
                    raise SystemExit(f"Unknown string constant {prefix_name} in {path}")
                upstream_id = constants[prefix_name] + upstream_id
            link = {
                "upstreamId": upstream_id,
                "dotnetTest": f"Vercel.AI.Tests.{class_name}.{method.group(1)}",
                "coverage": coverage.group(1) if coverage else "Partial",
            }
            if note:
                link["note"] = unescape(note.group(1))
            links.append(link)
            cursor = match.end()
    seen: dict[str, str] = {}
    duplicates: list[str] = []
    for link in links:
        previous = seen.get(link["upstreamId"])
        if previous is not None:
            duplicates.append(f"{link['upstreamId']}\n  {previous}\n  {link['dotnetTest']}")
        else:
            seen[link["upstreamId"]] = link["dotnetTest"]
    if duplicates:
        raise SystemExit("Duplicate upstream ids:\n" + "\n".join(duplicates))
    return links


def load_catalog() -> tuple[list[dict[str, object]], dict[str, object]]:
    rows = [
        json.loads(line)
        for line in (PARITY / "upstream-unit-tests.jsonl").read_text(encoding="utf-8").splitlines()
        if line
    ]
    manifest = json.loads((PARITY / "manifest.json").read_text(encoding="utf-8"))
    return rows, manifest


def summarize(rows: list[dict[str, object]], links: list[dict[str, str]], commit: str) -> dict[str, object]:
    linked = {link["upstreamId"]: link for link in links}
    features: dict[str, dict[str, object]] = {}
    covered = partial = missing = 0
    for row in rows:
        if row["scope"] != "in-scope":
            continue
        feature_name = str(row["feature"])
        feature = features.get(feature_name)
        if feature is None:
            feature = {
                "feature": feature_name,
                "dotnetProject": row.get("dotnetProject") or "",
                "inScope": 0,
                "covered": 0,
                "partial": 0,
                "missing": 0,
            }
            features[feature_name] = feature
        feature["inScope"] = int(feature["inScope"]) + 1
        link = linked.get(str(row["id"]))
        if link is None:
            feature["missing"] = int(feature["missing"]) + 1
            missing += 1
        elif link["coverage"] == "Covered":
            feature["covered"] = int(feature["covered"]) + 1
            covered += 1
        else:
            feature["partial"] = int(feature["partial"]) + 1
            partial += 1
    in_scope = sum(1 for row in rows if row["scope"] == "in-scope")
    return {
        "upstreamCommit": commit,
        "unitTests": len(rows),
        "inScope": in_scope,
        "outOfScope": len(rows) - in_scope,
        "covered": covered,
        "partial": partial,
        "missing": missing,
        "features": [features[name] for name in sorted(features)],
    }


def issue_body(feature: dict[str, object], rows: list[dict[str, object]], links: list[dict[str, str]], commit: str) -> str:
    name = str(feature["feature"])
    feature_rows = [row for row in rows if row.get("feature") == name and row.get("scope") == "in-scope"]
    by_id = {link["upstreamId"]: link for link in links}
    partials = []
    missing_by_file: dict[str, list[dict[str, object]]] = defaultdict(list)
    for row in feature_rows:
        link = by_id.get(str(row["id"]))
        if link is None:
            missing_by_file[str(row["file"])].append(row)
        elif link["coverage"] != "Covered":
            partials.append((row, link))

    lines = [
        f"Implement the upstream unit tests for `{name}` in `{feature['dotnetProject']}`.",
        "",
        f"Parity target: [vercel/ai@{commit[:12]}](https://github.com/vercel/ai/tree/{commit}) (`COMPATIBILITY.md`).",
        "",
        "The checklist is `tests/parity/upstream-unit-tests.jsonl`, filtered by this feature. "
        "When a .NET test asserts the same behavior, mark the upstream id on that method:",
        "",
        "```csharp",
        '[UpstreamTest("packages/.../file.test.ts::suite::title", Coverage = UpstreamCoverage.Covered)]',
        "```",
        "",
        "`UpstreamParityTests` fails if the id is unknown or `tests/parity/coverage-summary.json` is stale. "
        "Rebuild the summary with `python3 build/parity-report.py`.",
        "",
        "Do not copy the TypeScript. Reimplement the public behavior and assert the same outcomes.",
        "",
        f"In scope: {feature['inScope']}. Covered: {feature['covered']}. Partial: {feature['partial']}. Missing: {feature['missing']}.",
    ]
    if partials:
        lines.extend(["", "## Partial", ""])
        for row, link in partials:
            note = f" — {link['note']}" if link.get("note") else ""
            lines.append(f"- `{row['id']}`")
            lines.append(f"  - `{link['dotnetTest']}` ({link['coverage']}){note}")
    lines.extend(["", "## Missing", ""])
    for file_name in sorted(missing_by_file):
        cases = missing_by_file[file_name]
        lines.append(f"### `{file_name}` ({len(cases)})")
        lines.append("")
        shown = cases if len(cases) <= 40 else cases[:40]
        for row in shown:
            suite = f"{row['suite']} — " if row.get("suite") else ""
            runtime = " [node]" if row.get("runtime") == "node" else ""
            lines.append(f"- line {row['line']}: {suite}{row['title']}{runtime}")
        if len(cases) > len(shown):
            lines.append(f"- {len(cases) - len(shown)} more in this file; filter the catalog by `{file_name}`.")
        lines.append("")
    body = "\n".join(lines).rstrip() + "\n"
    if len(body) > 60000:
        # Keep the file index and drop per-test lines for the largest files.
        lines = [line for line in lines if not line.startswith("- line ")]
        lines.append("The per-test titles were omitted because the list exceeds the GitHub issue size. Filter the catalog by this feature.")
        lines.append("")
        body = "\n".join(lines).rstrip() + "\n"
    return body


def write_issue_index(summary: dict[str, object]) -> None:
    path = PARITY / "issues.json"
    existing: dict[str, str] = {}
    if path.exists():
        previous = json.loads(path.read_text(encoding="utf-8"))
        for issue in previous.get("issues", []):
            url = issue.get("url")
            if url:
                existing[str(issue["feature"])] = str(url)
    issues = []
    for feature in summary["features"]:
        if int(feature["missing"]) == 0 and int(feature["partial"]) == 0 and feature["feature"] not in existing:
            continue
        issues.append(
            {
                "feature": feature["feature"],
                "title": f"Cover upstream unit tests: {feature['feature']}",
                "dotnetProject": feature["dotnetProject"],
                "inScope": feature["inScope"],
                "covered": feature["covered"],
                "partial": feature["partial"],
                "missing": feature["missing"],
                "url": existing.get(str(feature["feature"])),
            }
        )
    path.write_text(json.dumps({"issues": issues}, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--issues", type=Path, help="Write one JSON object per feature issue")
    args = parser.parse_args()
    rows, manifest = load_catalog()
    links = load_links()
    known = {str(row["id"]) for row in rows}
    missing_ids = [link["upstreamId"] for link in links if link["upstreamId"] not in known]
    if missing_ids:
        raise SystemExit("Unknown upstream ids:\n" + "\n".join(missing_ids))
    summary = summarize(rows, links, str(manifest["upstreamCommit"]))
    (PARITY / "coverage-summary.json").write_text(
        json.dumps(summary, indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8",
    )
    write_issue_index(summary)
    print(
        f"coverage: {summary['covered']} covered, {summary['partial']} partial, "
        f"{summary['missing']} missing across {len(summary['features'])} features"
    )
    if args.issues:
        commit = str(manifest["upstreamCommit"])
        with args.issues.open("w", encoding="utf-8") as handle:
            for feature in summary["features"]:
                if int(feature["missing"]) == 0 and int(feature["partial"]) == 0:
                    continue
                payload = {
                    "feature": feature["feature"],
                    "title": f"Cover upstream unit tests: {feature['feature']}",
                    "body": issue_body(feature, rows, links, commit),
                }
                handle.write(json.dumps(payload, ensure_ascii=False) + "\n")
        print(f"wrote issue payloads to {args.issues}")


if __name__ == "__main__":
    main()
