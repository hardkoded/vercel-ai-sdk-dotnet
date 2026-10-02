#!/usr/bin/env python3
"""Collect vitest unit tests from a vercel/ai checkout.

Writes tests/parity/upstream-unit-tests.jsonl and tests/parity/manifest.json.
The catalog is the parity snapshot for the commit recorded in COMPATIBILITY.md.
Integration and end-to-end files are listed in the manifest and omitted from
the catalog. Packages that COMPATIBILITY.md does not port stay in the catalog
with scope "out-of-scope" so the inventory is complete.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

NOT_PORTED = {
    "react",
    "vue",
    "svelte",
    "angular",
    "rsc",
    "valibot",
    "langchain",
    "llamaindex",
    "devtools",
    "codemod",
    "test-server",
    "tui",
    "policy-opa",
    "code-mode",
    "typesafe-ai",
}

HOOKS = {"beforeEach", "afterEach", "beforeAll", "afterAll"}

# JavaScript package name -> .NET project. None means the package is not ported.
DOTNET_PROJECTS = {
    "ai": "Vercel.AI",
    "provider": "Vercel.AI.Provider",
    "provider-utils": "Vercel.AI.ProviderUtils",
    "gateway": "Vercel.AI.Gateway",
    "openai-compatible": "Vercel.AI.OpenAICompatible",
    "openai": "Vercel.AI.OpenAI",
    "anthropic": "Vercel.AI.Anthropic",
    "anthropic-aws": "Vercel.AI.Anthropic",
    "google": "Vercel.AI.Google",
    "google-vertex": "Vercel.AI.Google",
    "amazon-bedrock": "Vercel.AI.AmazonBedrock",
    "azure": "Vercel.AI.Azure",
    "cohere": "Vercel.AI.Cohere",
    "mistral": "Vercel.AI.Mistral",
    "alibaba": "Vercel.AI.Alibaba",
    "assemblyai": "Vercel.AI.AssemblyAI",
    "baseten": "Vercel.AI.Baseten",
    "black-forest-labs": "Vercel.AI.BlackForestLabs",
    "bytedance": "Vercel.AI.ByteDance",
    "cartesia": "Vercel.AI.Cartesia",
    "cerebras": "Vercel.AI.Cerebras",
    "deepgram": "Vercel.AI.Deepgram",
    "deepinfra": "Vercel.AI.DeepInfra",
    "deepseek": "Vercel.AI.DeepSeek",
    "elevenlabs": "Vercel.AI.ElevenLabs",
    "fal": "Vercel.AI.Fal",
    "fireworks": "Vercel.AI.Fireworks",
    "fish-audio": "Vercel.AI.FishAudio",
    "gladia": "Vercel.AI.Gladia",
    "gmicloud": "Vercel.AI.GmiCloud",
    "groq": "Vercel.AI.Groq",
    "huggingface": "Vercel.AI.HuggingFace",
    "hume": "Vercel.AI.Hume",
    "klingai": "Vercel.AI.KlingAI",
    "luma": "Vercel.AI.Luma",
    "minimax": "Vercel.AI.MiniMax",
    "moonshotai": "Vercel.AI.Moonshot",
    "open-responses": "Vercel.AI.OpenResponses",
    "otel": "Vercel.AI.OpenTelemetry",
    "perplexity": "Vercel.AI.Perplexity",
    "prodia": "Vercel.AI.Prodia",
    "quiverai": "Vercel.AI.QuiverAI",
    "replicate": "Vercel.AI.Replicate",
    "revai": "Vercel.AI.RevAI",
    "togetherai": "Vercel.AI.TogetherAI",
    "voyage": "Vercel.AI.Voyage",
    "xai": "Vercel.AI.Xai",
    "zai": "Vercel.AI.Zai",
    "mcp": "Vercel.AI.Mcp",
}


def not_ported(package: str) -> bool:
    if package in NOT_PORTED:
        return True
    return package.startswith(("harness", "workflow", "sandbox-"))


def is_ident_char(ch: str) -> bool:
    return ch.isalnum() or ch in "_$"


# A slash after one of these tokens starts a regex literal, not division.
REGEX_PREV_CHARS = set("([{:;,=!?&|~^%<>+-*}")
REGEX_PREV_WORDS = {
    "return",
    "throw",
    "case",
    "typeof",
    "void",
    "delete",
    "await",
    "yield",
    "else",
    "do",
    "in",
    "of",
    "instanceof",
}


class Scanner:
    def __init__(self, text: str) -> None:
        self.s = text
        self.n = len(text)
        self.i = 0
        self.line = 1

    def eof(self) -> bool:
        return self.i >= self.n

    def ch(self, k: int = 0) -> str:
        j = self.i + k
        return self.s[j] if 0 <= j < self.n else ""

    def adv(self, count: int = 1) -> None:
        for _ in range(count):
            if self.eof():
                return
            if self.s[self.i] == "\n":
                self.line += 1
            self.i += 1

    def skip_ws_comments(self) -> None:
        while not self.eof():
            c = self.ch()
            if c in " \t\r\n":
                self.adv()
                continue
            if c == "/" and self.ch(1) == "/":
                while not self.eof() and self.ch() != "\n":
                    self.adv()
                continue
            if c == "/" and self.ch(1) == "*":
                self.adv(2)
                while not self.eof() and not (self.ch() == "*" and self.ch(1) == "/"):
                    self.adv()
                if not self.eof():
                    self.adv(2)
                continue
            return

    def skip_string(self, quote: str) -> None:
        self.adv()
        if quote == "`":
            self._skip_template_body()
            return
        while not self.eof():
            c = self.ch()
            if c == "\\":
                self.adv(2)
                continue
            if c == quote:
                self.adv()
                return
            self.adv()

    def _skip_template_body(self) -> None:
        while not self.eof():
            c = self.ch()
            if c == "\\":
                self.adv(2)
                continue
            if c == "`":
                self.adv()
                return
            if c == "$" and self.ch(1) == "{":
                self.adv(2)
                self._skip_balanced_brace()
                continue
            self.adv()

    def _skip_balanced_brace(self) -> None:
        depth = 1
        while not self.eof() and depth:
            c = self.ch()
            if c in " \t\r\n":
                self.adv()
                continue
            if c == "/":
                self.consume_slash()
                continue
            if c in "\"'`":
                self.skip_string(c)
                continue
            if c == "{":
                depth += 1
                self.adv()
                continue
            if c == "}":
                depth -= 1
                self.adv()
                continue
            self.adv()

    def read_ident(self) -> str:
        start = self.i
        if not self.eof() and (self.ch().isalpha() or self.ch() in "_$"):
            self.adv()
            while not self.eof() and is_ident_char(self.ch()):
                self.adv()
        return self.s[start:self.i]

    def read_string(self) -> str | None:
        quote = self.ch()
        if quote not in "\"'`":
            return None
        self.adv()
        if quote == "`":
            return self._read_template_body()
        chars: list[str] = []
        while not self.eof():
            c = self.ch()
            if c == "\\":
                self.adv()
                if self.eof():
                    break
                esc = self.ch()
                chars.append(
                    {
                        "n": "\n",
                        "t": "\t",
                        "r": "\r",
                        "\\": "\\",
                        "'": "'",
                        '"': '"',
                        "`": "`",
                    }.get(esc, esc)
                )
                self.adv()
                continue
            if c == quote:
                self.adv()
                return "".join(chars)
            chars.append(c)
            self.adv()
        return "".join(chars)

    def _read_template_body(self) -> str:
        chars: list[str] = []
        while not self.eof():
            c = self.ch()
            if c == "\\":
                self.adv()
                if self.eof():
                    break
                chars.append(self.ch())
                self.adv()
                continue
            if c == "`":
                self.adv()
                return "".join(chars)
            if c == "$" and self.ch(1) == "{":
                chars.append("${")
                self.adv(2)
                start = self.i
                self._skip_balanced_brace()
                chars.append(self.s[start:self.i])
                continue
            chars.append(c)
            self.adv()
        return "".join(chars)

    def previous_token(self) -> str:
        j = self.i - 1
        while j >= 0 and self.s[j] in " \t\r\n":
            j -= 1
        if j >= 1 and self.s[j] == "/" and self.s[j - 1] == "*":
            j -= 2
            while j >= 1 and not (self.s[j] == "/" and self.s[j - 1] == "*"):
                j -= 1
            j -= 2
            while j >= 0 and self.s[j] in " \t\r\n":
                j -= 1
        if j < 0:
            return ""
        ch = self.s[j]
        if is_ident_char(ch):
            end = j
            while j >= 0 and is_ident_char(self.s[j]):
                j -= 1
            return self.s[j + 1 : end + 1]
        return ch

    def is_regex_start(self) -> bool:
        prev = self.previous_token()
        return prev == "" or prev in REGEX_PREV_CHARS or prev in REGEX_PREV_WORDS

    def skip_regex(self) -> None:
        """Consume a /.../flags literal. Current character is the opening slash."""
        self.adv()
        in_class = False
        while not self.eof():
            c = self.ch()
            if c == "\\":
                self.adv(2)
                continue
            if c == "\n":
                return
            if c == "[":
                in_class = True
            elif c == "]" and in_class:
                in_class = False
            elif c == "/" and not in_class:
                self.adv()
                while self.ch().isalnum() or self.ch() == "_":
                    self.adv()
                return
            self.adv()

    def consume_slash(self) -> None:
        """Consume a comment, regex literal, or division operator."""
        if self.ch(1) in "/*":
            self.skip_ws_comments()
            return
        if self.is_regex_start():
            self.skip_regex()
            return
        self.adv()

    def skip_balanced_paren(self) -> None:
        if self.ch() != "(":
            return
        depth = 0
        while not self.eof():
            c = self.ch()
            if c in " \t\r\n":
                self.adv()
                continue
            if c == "/":
                self.consume_slash()
                continue
            if c in "\"'`":
                self.skip_string(c)
                continue
            if c == "(":
                depth += 1
                self.adv()
                continue
            if c == ")":
                depth -= 1
                self.adv()
                if depth == 0:
                    return
                continue
            self.adv()


def _collapse(title: str) -> str:
    return " ".join(title.split())


def parse_call(ts: Scanner, brace: list[int]) -> tuple[str | None, int, bool]:
    """Parse it/test/describe arguments. Current character is '('.

    When the callback body starts, brace[0] is incremented and the scanner is
    left just after that '{'. The returned flag is true only in that case.
    """
    paren = 0
    seen_arrow = False
    title: str | None = None
    title_line = ts.line
    while not ts.eof():
        c = ts.ch()
        if c in " \t\r\n":
            ts.adv()
            continue
        if c == "/":
            ts.consume_slash()
            continue
        if c in "\"'`":
            if paren == 1 and title is None:
                title_line = ts.line
                title = ts.read_string()
            else:
                ts.skip_string(c)
            continue
        if c == "(":
            paren += 1
            ts.adv()
            continue
        if c == ")":
            paren -= 1
            ts.adv()
            if paren == 0:
                return title, title_line, False
            continue
        if c == "{":
            if paren == 1 and seen_arrow:
                brace[0] += 1
                ts.adv()
                return title, title_line, True
            ts._skip_balanced_brace_from_open()
            continue
        if c == "=" and ts.ch(1) == ">":
            seen_arrow = True
            ts.adv(2)
            continue
        if c.isalpha() or c in "_$":
            word = ts.read_ident()
            if word == "function":
                seen_arrow = True
            continue
        ts.adv()
    return title, title_line, False


def _skip_balanced_brace_from_open(self: Scanner) -> None:
    if self.ch() != "{":
        return
    self.adv()
    self._skip_balanced_brace()


Scanner._skip_balanced_brace_from_open = _skip_balanced_brace_from_open  # type: ignore[attr-defined]


def parse_invocation(ts: Scanner, brace: list[int]) -> tuple[list[str], str | None, int, bool] | None:
    """Parse modifiers and the call. Current position is just after the ident."""
    modifiers: list[str] = []
    while True:
        ts.skip_ws_comments()
        if ts.ch() == ".":
            ts.adv()
            ts.skip_ws_comments()
            mod = ts.read_ident()
            if not mod:
                return None
            modifiers.append(mod)
            ts.skip_ws_comments()
            if ts.ch() == "(":
                if HOOKS.intersection(modifiers):
                    ts.skip_balanced_paren()
                    return None
                ts.skip_balanced_paren()
                continue
            if ts.ch() == "`" and mod == "each":
                ts.skip_string("`")
                continue
            continue
        if ts.ch() == "(":
            if HOOKS.intersection(modifiers):
                ts.skip_balanced_paren()
                return None
            title, title_line, entered = parse_call(ts, brace)
            return modifiers, title, title_line, entered
        return None


def extract_tests(text: str) -> list[dict[str, object]]:
    ts = Scanner(text)
    brace = [0]
    describes: list[tuple[str, int]] = []
    found: list[dict[str, object]] = []
    while not ts.eof():
        c = ts.ch()
        if c in " \t\r\n":
            ts.adv()
            continue
        if c == "/":
            ts.consume_slash()
            continue
        if c in "\"'`":
            ts.skip_string(c)
            continue
        if c == "{":
            brace[0] += 1
            ts.adv()
            continue
        if c == "}":
            brace[0] -= 1
            while describes and brace[0] < describes[-1][1]:
                describes.pop()
            ts.adv()
            continue
        prev = ts.s[ts.i - 1] if ts.i else ""
        if (c.isalpha() or c in "_$") and not is_ident_char(prev):
            line = ts.line
            word = ts.read_ident()
            # `pattern.test(` is RegExp.test, not a vitest call.
            if prev != "." and word in {"it", "test", "describe"}:
                parsed = parse_invocation(ts, brace)
                if parsed is None:
                    continue
                modifiers, title, title_line, entered = parsed
                label = _collapse(title) if title else "<dynamic>"
                if word == "describe":
                    if entered:
                        describes.append((label, brace[0]))
                    continue
                suite = [name for name, _depth in describes]
                found.append(
                    {
                        "line": title_line or line,
                        "title": label,
                        "suite": suite,
                        "modifiers": modifiers,
                        "kind": word,
                    }
                )
            continue
        ts.adv()
    return found


def feature_for(package: str, relative_file: str) -> str:
    """Feature is the package plus the directory of the test file.

    The src/ segment is omitted so packages/ai/src/generate-text/stop-condition.test.ts
    is ai/generate-text. Files that live beside src, such as packages/ai/internal,
    stay under ai/internal.
    """
    prefix = f"packages/{package}/"
    tail = relative_file[len(prefix) :] if relative_file.startswith(prefix) else relative_file
    if tail.startswith("src/"):
        tail = tail[4:]
    parent = str(Path(tail).parent)
    if parent == ".":
        return package
    return f"{package}/{parent}"


def scope_for(package: str) -> tuple[str, str | None, str | None]:
    if not_ported(package):
        return "out-of-scope", "package-not-ported", None
    project = DOTNET_PROJECTS.get(package)
    if project is None:
        return "out-of-scope", "no-dotnet-project", None
    return "in-scope", None, project


def is_excluded_file(path: Path) -> str | None:
    name = path.name
    if name.endswith(".integration.test.ts") or ".integration.test." in name:
        return "integration"
    if name.endswith(".e2e.test.ts") or ".e2e.test." in name:
        return "e2e"
    if "/e2e/" in path.as_posix():
        return "e2e"
    return None


def collect(checkout: Path) -> tuple[list[dict[str, object]], dict[str, object]]:
    packages = checkout / "packages"
    if not packages.is_dir():
        raise SystemExit(f"No packages directory at {packages}")
    commit = (checkout / ".git")
    sha = _rev_parse(checkout)
    rows: list[dict[str, object]] = []
    excluded: list[dict[str, str]] = []
    seen: dict[str, int] = {}
    files = sorted(
        p
        for p in packages.rglob("*")
        if p.is_file() and (p.name.endswith(".test.ts") or p.name.endswith(".test.tsx"))
    )
    for path in files:
        reason = is_excluded_file(path)
        relative = path.relative_to(checkout).as_posix()
        if reason:
            excluded.append({"file": relative, "reason": reason})
            continue
        package = relative.split("/")[1]
        text = path.read_text(encoding="utf-8", errors="replace")
        runtime = "node" if path.name.endswith(".node.test.ts") else None
        scope, scope_reason, dotnet = scope_for(package)
        for test in extract_tests(text):
            suite = test["suite"]
            assert isinstance(suite, list)
            title = str(test["title"])
            suite_path = " > ".join(str(part) for part in suite)
            base = f"{relative}::{suite_path}::{title}" if suite_path else f"{relative}::{title}"
            seen[base] = seen.get(base, 0) + 1
            test_id = base if seen[base] == 1 else f"{base} #{seen[base]}"
            modifiers = test["modifiers"]
            assert isinstance(modifiers, list)
            row: dict[str, object] = {
                "id": test_id,
                "package": package,
                "feature": feature_for(package, relative),
                "file": relative,
                "line": test["line"],
                "title": title,
                "kind": test["kind"],
                "scope": scope,
            }
            if suite_path:
                row["suite"] = suite_path
            if modifiers:
                row["modifiers"] = modifiers
            if runtime:
                row["runtime"] = runtime
            if scope_reason:
                row["scopeReason"] = scope_reason
            if dotnet:
                row["dotnetProject"] = dotnet
            rows.append(row)
    rows.sort(key=lambda row: (str(row["file"]), int(row["line"]), str(row["id"])))
    manifest: dict[str, object] = {
        "upstreamRepo": "https://github.com/vercel/ai",
        "upstreamCommit": sha,
        "unitTestCount": len(rows),
        "inScopeCount": sum(1 for row in rows if row["scope"] == "in-scope"),
        "outOfScopeCount": sum(1 for row in rows if row["scope"] == "out-of-scope"),
        "excludedFiles": excluded,
        "dotnetProjects": DOTNET_PROJECTS,
        "notPortedPackages": sorted(NOT_PORTED),
    }
    return rows, manifest


def _rev_parse(checkout: Path) -> str:
    head = checkout / ".git"
    git_dir = head
    if head.is_file():
        line = head.read_text(encoding="utf-8").strip()
        if line.startswith("gitdir:"):
            git_dir = Path(line.split(":", 1)[1].strip())
            if not git_dir.is_absolute():
                git_dir = (checkout / git_dir).resolve()
    head_file = git_dir / "HEAD"
    if not head_file.is_file():
        raise SystemExit(f"Cannot read HEAD for {checkout}")
    value = head_file.read_text(encoding="utf-8").strip()
    if value.startswith("ref:"):
        ref = git_dir / value.split(":", 1)[1].strip()
        return ref.read_text(encoding="utf-8").strip()
    return value


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("checkout", type=Path, help="Path to a vercel/ai checkout")
    parser.add_argument(
        "--out",
        type=Path,
        default=Path(__file__).resolve().parents[1] / "tests" / "parity",
    )
    args = parser.parse_args()
    rows, manifest = collect(args.checkout.resolve())
    args.out.mkdir(parents=True, exist_ok=True)
    catalog = args.out / "upstream-unit-tests.jsonl"
    with catalog.open("w", encoding="utf-8", newline="\n") as handle:
        for row in rows:
            handle.write(json.dumps(row, ensure_ascii=False, separators=(",", ":")))
            handle.write("\n")
    (args.out / "manifest.json").write_text(
        json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8",
    )
    print(
        f"Wrote {len(rows)} unit tests "
        f"({manifest['inScopeCount']} in scope, {manifest['outOfScopeCount']} out of scope) "
        f"from {manifest['upstreamCommit']}"
    )


if __name__ == "__main__":
    main()
