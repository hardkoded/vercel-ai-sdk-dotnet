#!/usr/bin/env python3
"""Check that a port covers the unit tests an upstream commit adds or changes.

Lists the vitest tests that <upstream-sha> adds or changes in *.test.ts files,
using the ids of tests/parity/upstream-unit-tests.jsonl, and compares them with
the [UpstreamTest] links of the .NET tests and the "Not ported" section of the
PR body. Each upstream test is reported as one of:

  COVERED        a .NET test links it with Coverage = Covered
  PARTIAL        a .NET test links it with Coverage = Partial
  ACKNOWLEDGED   the PR body lists it under a "Not ported" heading
  MISSING        none of the above

The exit code is 1 when any test is MISSING. Tests in packages that the
catalog marks out of scope, and in integration or e2e files, are listed apart
and never fail the check.

By default the links come from the current working tree. With --pr they also
come from the added lines of the pull request diff, and the acknowledged gaps
from its body (needs `gh auth login`). With --base they also come from the
diff against that ref, and no PR body is read. Run it from a tree that is up
to date with main, so tests linked before the PR count as linked.

A test that already had a link counts as linked, even when the commit changed
it: the script cannot tell whether the .NET test follows the change. Review the
rows marked "changed" by hand.

A changed test is a test whose lines overlap a changed line of the commit. The
range of a test runs to the line before the next test, so a helper edited
between two tests marks the test above it.
"""

from __future__ import annotations

import argparse
import importlib.util
import re
import subprocess
import sys
from pathlib import Path

BUILD = Path(__file__).resolve().parent
REPO = "hardkoded/vercel-ai-sdk-dotnet"
DEFAULT_UPSTREAM = Path("/Users/dario/Code/vercel/ai")
HUNK = re.compile(r"^@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@", re.M)
HEADING = re.compile(r"^\s{0,3}#{1,6}\s+(.*)$")


def load_module(name: str, filename: str):
    """Import a sibling script. Their file names have hyphens."""
    spec = importlib.util.spec_from_file_location(name, BUILD / filename)
    module = importlib.util.module_from_spec(spec)
    sys.modules[name] = module
    spec.loader.exec_module(module)
    return module


collector = load_module("collect_upstream_tests", "collect-upstream-tests.py")
report = load_module("parity_report", "parity-report.py")


def run(args: list[str]) -> str:
    result = subprocess.run(args, capture_output=True, text=True, encoding="utf-8")
    if result.returncode != 0:
        raise SystemExit(f"{' '.join(args)} failed:\n{result.stderr.strip()}")
    return result.stdout


def git_show(repo: Path, rev: str, path: str) -> str | None:
    result = subprocess.run(
        ["git", "-C", str(repo), "show", f"{rev}:{path}"],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return result.stdout if result.returncode == 0 else None


def file_tests(relative: str, text: str) -> list[dict[str, object]]:
    """Parse one test file into rows with the same ids as the catalog."""
    seen: dict[str, int] = {}
    tests: list[dict[str, object]] = []
    for test in collector.extract_tests(text):
        title = str(test["title"])
        suite_path = " > ".join(str(part) for part in test["suite"])
        base = f"{relative}::{suite_path}::{title}" if suite_path else f"{relative}::{title}"
        seen[base] = seen.get(base, 0) + 1
        test_id = base if seen[base] == 1 else f"{base} #{seen[base]}"
        tests.append({"id": test_id, "line": int(test["line"]), "title": title, "suite": suite_path})
    return tests


def changed_lines(repo: Path, sha: str, path: str) -> list[tuple[int, int]]:
    diff = run(["git", "-C", str(repo), "diff", "-U0", "--no-renames", f"{sha}^", sha, "--", path])
    ranges = []
    for match in HUNK.finditer(diff):
        start = int(match.group(1))
        count = int(match.group(2)) if match.group(2) is not None else 1
        if count:
            ranges.append((start, start + count - 1))
    return ranges


def commit_tests(repo: Path, sha: str) -> list[dict[str, object]]:
    """Tests the commit adds or changes. `how` is added or changed."""
    run(["git", "-C", str(repo), "rev-parse", "--verify", f"{sha}^{{commit}}"])
    names = run(
        [
            "git", "-C", str(repo), "diff", "--name-only", "--no-renames", "--diff-filter=AM",
            f"{sha}^", sha, "--", "*.test.ts", "*.test.tsx",
        ]
    ).splitlines()
    found: list[dict[str, object]] = []
    for path in names:
        new_text = git_show(repo, sha, path)
        if new_text is None:
            continue
        new_tests = file_tests(path, new_text)
        old_text = git_show(repo, f"{sha}^", path)
        old_ids = {test["id"] for test in file_tests(path, old_text)} if old_text else set()
        ranges = changed_lines(repo, sha, path)
        for index, test in enumerate(new_tests):
            start = int(test["line"])
            end = int(new_tests[index + 1]["line"]) - 1 if index + 1 < len(new_tests) else 10**9
            if test["id"] not in old_ids:
                how = "added"
            elif any(low <= end and high >= start for low, high in ranges):
                how = "changed"
            else:
                continue
            found.append({**test, "file": path, "how": how})
    return found


def scope_of(path: str) -> str:
    """in-scope, or the reason the catalog leaves the file out of the port."""
    reason = collector.is_excluded_file(Path(path))
    if reason:
        return reason
    parts = path.split("/")
    if parts[0] != "packages" or len(parts) < 2:
        return "out-of-scope"
    return collector.scope_for(parts[1])[0]


def diff_links(diff: str) -> list[dict[str, str]]:
    """Links from the added lines of a diff, file by file so that each file's
    constants resolve its own prefixes. A prefix the diff does not define
    cannot be resolved, so that link matches by id suffix."""
    links = []
    for section in re.split(r"^diff --git ", diff, flags=re.M):
        added = "\n".join(
            line[1:] for line in section.splitlines() if line.startswith("+") and not line.startswith("+++")
        )
        constants = dict(report.CONST_RE.findall(added))
        for match in report.ATTR.finditer(added):
            upstream_id = report.unescape(match.group("id"))
            prefix = match.group("prefix")
            link = {"coverage": "Covered" if "UpstreamCoverage.Covered" in match.group("rest") else "Partial"}
            if prefix and prefix not in constants:
                link["suffix"] = upstream_id
            else:
                resolved = ""
                if prefix:
                    resolved = "".join(
                        report.unescape(literal[1:-1]) if literal else constants.get(ident, "")
                        for literal, ident in report.TERM_RE.findall(constants[prefix])
                    )
                link["upstreamId"] = resolved + upstream_id
            links.append(link)
    return links


def tree_links() -> list[dict[str, str]]:
    return [{"upstreamId": link["upstreamId"], "coverage": link["coverage"]} for link in report.load_links()]


def find_link(test_id: str, links: list[dict[str, str]]) -> dict[str, str] | None:
    for link in links:
        if link.get("upstreamId") == test_id or (link.get("suffix") and test_id.endswith(link["suffix"])):
            return link
    return None


def not_ported_text(body: str) -> str:
    """Text under every heading that contains "Not ported", up to the next heading."""
    lines: list[str] = []
    inside = False
    for line in body.splitlines():
        heading = HEADING.match(line)
        if heading:
            inside = "not ported" in heading.group(1).lower()
        elif inside:
            lines.append(line)
    return " ".join("\n".join(lines).lower().split())


def acknowledged(test: dict[str, object], text: str) -> bool:
    """Loose match: the test title or its describe name appears in the section."""
    names = [str(test["title"])]
    if test["suite"]:
        names += [str(test["suite"]), str(test["suite"]).split(" > ")[-1]]
    return any(" ".join(name.lower().split()) in text for name in names)


def main() -> None:
    parser = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument("sha", help="Upstream commit in the vercel/ai clone")
    parser.add_argument(
        "--upstream-repo",
        type=Path,
        default=DEFAULT_UPSTREAM,
        help=f"Path to a vercel/ai clone that has the commit (default: {DEFAULT_UPSTREAM})",
    )
    source = parser.add_mutually_exclusive_group()
    source.add_argument("--pr", type=int, help="Pull request number: read its diff and its Not ported section")
    source.add_argument("--base", help="Git ref: read [UpstreamTest] links added since it (git diff REF...HEAD)")
    args = parser.parse_args()

    repo = args.upstream_repo.resolve()
    if not (repo / ".git").exists():
        raise SystemExit(f"No git checkout at {repo}. Pass --upstream-repo.")

    ack = ""
    if args.pr:
        links = tree_links() + diff_links(run(["gh", "pr", "diff", str(args.pr), "--repo", REPO]))
        body = run(["gh", "pr", "view", str(args.pr), "--repo", REPO, "--json", "body", "-q", ".body"])
        ack = not_ported_text(body)
    elif args.base:
        links = tree_links() + diff_links(run(["git", "-C", str(BUILD.parent), "diff", f"{args.base}...HEAD", "--", "tests"]))
    else:
        links = tree_links()

    rows = []
    skipped = []
    for test in commit_tests(repo, args.sha):
        scope = scope_of(str(test["file"]))
        if scope != "in-scope":
            skipped.append((test, scope))
            continue
        link = find_link(str(test["id"]), links)
        if link:
            status = "COVERED" if link["coverage"] == "Covered" else "PARTIAL"
        elif ack and acknowledged(test, ack):
            status = "ACKNOWLEDGED"
        else:
            status = "MISSING"
        rows.append((status, test))

    print(f"Upstream commit {args.sha}: {len(rows)} in-scope tests added or changed, {len(skipped)} out of scope.\n")
    for status, test in rows:
        print(f"{status:<13} {test['how']:<8} {test['id']}")
    if skipped:
        print("\nOut of scope:")
        for test, scope in skipped:
            print(f"  ({scope}) {test['id']}")
    names = ("COVERED", "PARTIAL", "ACKNOWLEDGED", "MISSING")
    counts = {name: sum(1 for status, _ in rows if status == name) for name in names}
    print("\n" + ", ".join(f"{count} {name}" for name, count in counts.items()))
    sys.exit(1 if counts["MISSING"] else 0)


if __name__ == "__main__":
    main()
