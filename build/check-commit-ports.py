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


def hunk_ranges(diff: str) -> list[tuple[int, int]]:
    """New-side line ranges of a -U0 diff. A deletion-only hunk counts as the
    line before the deletion, which belongs to the test that lost lines."""
    ranges = []
    for match in HUNK.finditer(diff):
        start = int(match.group(1))
        count = int(match.group(2)) if match.group(2) is not None else 1
        ranges.append((start, start + count - 1) if count else (start, start))
    return ranges


def changed_lines(repo: Path, sha: str, path: str) -> list[tuple[int, int]]:
    return hunk_ranges(run(["git", "-C", str(repo), "diff", "-U0", "--no-renames", f"{sha}^", sha, "--", path]))


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


def resolve_constant(name: str, constants: dict[str, str], stack: tuple[str, ...] = ()) -> str | None:
    """Value of a C# string constant, or None when a name in its chain is unknown."""
    if name not in constants or name in stack:
        return None
    parts = []
    for literal, ident in report.TERM_RE.findall(constants[name]):
        value = report.unescape(literal[1:-1]) if literal else resolve_constant(ident, constants, stack + (name,))
        if value is None:
            return None
        parts.append(value)
    return "".join(parts)


def diff_links(diff: str) -> list[dict[str, str]]:
    """Links from the lines a diff adds, file by file so that each file's
    constants resolve its own prefixes. As in parity-report.py, an attribute
    counts only when a test method follows it, and a commented-out attribute
    does not count. Context lines are read too, so an attribute added above an
    unchanged method works. A prefix that cannot be resolved from the diff makes
    the link match by id suffix instead."""
    links = []
    for section in re.split(r"^diff --git ", diff, flags=re.M):
        text = ""
        added_spans = []
        for line in section.splitlines():
            if line.startswith("+") and not line.startswith("+++"):
                added_spans.append((len(text), len(text) + len(line)))
                text += line[1:] + "\n"
            elif line.startswith(" "):
                text += line[1:] + "\n"
        constants = dict(report.CONST_RE.findall(text))
        for match in report.ATTR.finditer(text):
            line_start = text.rfind("\n", 0, match.start()) + 1
            if text[line_start:match.start()].strip():
                continue  # commented out, or not at the start of a line
            if not any(low <= match.start() < high for low, high in added_spans):
                continue
            if report.METHOD_RE.search(text, match.end()) is None:
                continue
            upstream_id = report.unescape(match.group("id"))
            prefix = match.group("prefix")
            link = {"coverage": "Covered" if "UpstreamCoverage.Covered" in match.group("rest") else "Partial"}
            resolved = resolve_constant(prefix, constants) if prefix else ""
            if resolved is None:
                link["suffix"] = upstream_id
            else:
                link["upstreamId"] = resolved + upstream_id
            links.append(link)
    return links


def tree_links() -> list[dict[str, str]]:
    return [{"upstreamId": link["upstreamId"], "coverage": link["coverage"]} for link in report.load_links()]


def find_link(test_id: str, links: list[dict[str, str]], ids: list[str]) -> tuple[dict[str, str] | None, bool]:
    """The link for a test id, and whether the match is ambiguous. A suffix
    link counts only when exactly one test of the commit ends with it."""
    for link in links:
        if link.get("upstreamId") == test_id:
            return link, False
    for link in links:
        suffix = link.get("suffix")
        if suffix and test_id.endswith(suffix):
            if sum(1 for other in ids if other.endswith(suffix)) > 1:
                return None, True
            return link, False
    return None, False


def normalize(name: str) -> str:
    return " ".join(name.replace("`", "").lower().split())


def not_ported_entries(body: str) -> set[str]:
    """Entries under every heading that contains "Not ported", up to the next
    heading. An entry is a backtick span, a whole bullet, or the bullet text
    before its first colon. A span "suite > title" also gives its last part."""
    entries: set[str] = set()
    inside = False
    for line in body.splitlines():
        heading = HEADING.match(line)
        if heading:
            inside = "not ported" in heading.group(1).lower()
            continue
        if not inside:
            continue
        text = re.sub(r"^\s*(?:[-*+]|\d+\.)\s*", "", line)
        candidates = re.findall(r"`([^`]+)`", text) + [text, re.split(r":\s", text)[0]]
        entries.update(normalize(candidate) for candidate in candidates)
    return entries


def acknowledged(test: dict[str, object], entries: set[str]) -> bool:
    """An entry is kept as written. A bare entry names a test title or a
    describe name (suite-wide). A qualified entry (`chain > title`,
    `chain::title`, or `outer > inner`) must be a tail of the test's own chain,
    so it never reaches the same title in another suite."""
    title = normalize(str(test["title"]))
    suite = normalize(str(test["suite"]))
    bare = {title}
    qualified = []
    if suite:
        bare.update({suite, suite.split(" > ")[-1]})
        qualified = [f"{suite} > {title}", f"{suite}::{title}", suite]
    for entry in entries:
        if " > " in entry or "::" in entry:
            if any(name == entry or name.endswith(" > " + entry) for name in qualified):
                return True
        elif entry in bare:
            return True
    return False


def self_test() -> None:
    """Checks of the diff and body logic that need no upstream clone."""
    assert hunk_ranges("@@ -10,2 +9,0 @@\n@@ -20 +18,3 @@\n") == [(9, 9), (18, 20)]
    diff = (
        "diff --git a/T.cs b/T.cs\n"
        + "+    private const string TestFile = \"a.test.ts\";\n"
        + "+    private const string Prefix = TestFile + \"::\";\n"
        + "+    [UpstreamTest(Prefix + \"ok\", Coverage = UpstreamCoverage.Covered)]\n"
        + "+    public void Ok() {}\n"
        + "+    // [UpstreamTest(\"commented\")]\n"
        + "+    [UpstreamTest(Unknown + \"far\")]\n"
        + " [Fact]\n"
        + " public void Far() {}\n"
        + "+    [UpstreamTest(\"orphan\")]\n"
    )
    ids = [link.get("upstreamId") or "~" + link["suffix"] for link in diff_links(diff)]
    assert ids == ["a.test.ts::ok", "~far"], ids
    entries = not_ported_entries("## Not ported\n- `should pass correct event information on success`: no hook\n- `suite A > should x`\n## Tests\n- other\n")
    test = {"title": "should pass correct event information", "suite": "suite B"}
    assert not acknowledged(test, entries)
    assert acknowledged({"title": "should pass correct event information on success", "suite": ""}, entries)
    assert acknowledged({"title": "should x", "suite": "suite A"}, entries)
    assert not acknowledged({"title": "other", "suite": ""}, entries)
    entries = not_ported_entries("## Not ported\n- `A > doGenerate > shell tool::should do it`\n- `doStream > shell tool::should stream`\n")
    assert acknowledged({"title": "should do it", "suite": "Root > A > doGenerate > shell tool"}, entries)
    assert not acknowledged({"title": "should do it", "suite": "Root > doStream > shell tool"}, entries)
    assert acknowledged({"title": "should stream", "suite": "Root > doStream > shell tool"}, entries)
    assert not acknowledged({"title": "should stream", "suite": "Root > doStream > other shell tool"}, entries)
    entries = not_ported_entries("## Not ported\n- `suite A > should x`\n")
    assert acknowledged({"title": "should x", "suite": "suite A"}, entries)
    assert not acknowledged({"title": "should x", "suite": "suite B"}, entries)
    assert not acknowledged({"title": "should x", "suite": ""}, entries)
    entries = not_ported_entries("## Not ported\n- `shell tool`\n- `should y`\n")
    assert acknowledged({"title": "z", "suite": "Root > shell tool"}, entries)
    assert acknowledged({"title": "should y", "suite": "Other"}, entries)
    one = [{"suffix": "::dup title", "coverage": "Covered"}]
    ids = ["a.test.ts::dup title", "b.test.ts::dup title", "c.test.ts::solo"]
    assert find_link(ids[0], one, ids) == (None, True)
    assert find_link(ids[0], one, ids[:1] + ids[2:])[0] == one[0]
    assert find_link(ids[1], [{"upstreamId": ids[1], "coverage": "Covered"}], ids)[0] is not None
    print("self-test passed")


def main() -> None:
    parser = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument("sha", nargs="?", help="Upstream commit in the vercel/ai clone")
    parser.add_argument("--self-test", action="store_true", help="Run the built-in checks of the diff and body logic and exit")
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
    if args.self_test:
        self_test()
        return
    if not args.sha:
        parser.error("the sha argument is required")

    repo = args.upstream_repo.resolve()
    if not (repo / ".git").exists():
        raise SystemExit(f"No git checkout at {repo}. Pass --upstream-repo.")

    ack = ""
    if args.pr:
        links = tree_links() + diff_links(run(["gh", "pr", "diff", str(args.pr), "--repo", REPO]))
        body = run(["gh", "pr", "view", str(args.pr), "--repo", REPO, "--json", "body", "-q", ".body"])
        ack = not_ported_entries(body)
    elif args.base:
        links = tree_links() + diff_links(run(["git", "-C", str(BUILD.parent), "diff", f"{args.base}...HEAD", "--", "tests"]))
    else:
        links = tree_links()

    rows = []
    skipped = []
    found = commit_tests(repo, args.sha)
    ids = [str(test["id"]) for test in found]
    for test in found:
        scope = scope_of(str(test["file"]))
        if scope != "in-scope":
            skipped.append((test, scope))
            continue
        link, ambiguous = find_link(str(test["id"]), links, ids)
        if ambiguous:
            status = "AMBIGUOUS"
        elif link:
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
    names = ("COVERED", "PARTIAL", "ACKNOWLEDGED", "AMBIGUOUS", "MISSING")
    counts = {name: sum(1 for status, _ in rows if status == name) for name in names}
    print("\n" + ", ".join(f"{count} {name}" for name, count in counts.items()))
    sys.exit(1 if counts["MISSING"] or counts["AMBIGUOUS"] else 0)


if __name__ == "__main__":
    main()
