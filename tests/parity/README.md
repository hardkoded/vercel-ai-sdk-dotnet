# Upstream unit-test parity

This directory tracks vitest unit tests from [vercel/ai](https://github.com/vercel/ai) against the .NET suite. The snapshot is the commit in `COMPATIBILITY.md`. Integration and end-to-end files are excluded and listed in `manifest.json`. Packages that `COMPATIBILITY.md` does not port are in the catalog with `"scope": "out-of-scope"`.

| File | Role |
| --- | --- |
| `upstream-unit-tests.jsonl` | One upstream `it` / `test` per line. `id` is `path::suite::title`. |
| `manifest.json` | Commit and excluded files. It holds no counts. Count the catalog (`build/parity-report.py` does) so PRs that add tests do not conflict here. |
| `generated/` | Ignored by git. `build/parity-report.py` writes `coverage-summary.json` (in-scope totals per feature) and `issues.json` (one GitHub issue per feature with missing or partial tests; `url` is filled by `build/open-upstream-test-issues.py`, which reuses an issue with the same title). |

A .NET test that asserts the same behavior links the upstream id:

```csharp
[UpstreamTest(
    "packages/ai/src/util/cosine-similarity.test.ts::should calculate cosine similarity correctly",
    Coverage = UpstreamCoverage.Partial,
    Note = "Asserts identical vectors only.")]
```

`Covered` means that upstream case is implemented. `Partial` means a .NET test overlaps it and the rest of the case is still open. An in-scope id with no attribute is missing. `python3 build/parity-report.py` writes both files into `generated/` on demand. They are not committed, so PRs that add tests do not conflict on them.

`python3 build/open-upstream-test-issues.py` opens one GitHub issue per feature that does not yet have a `url`. It needs `gh auth login` on `hardkoded/vercel-ai-sdk-dotnet`.

Before you open a port PR, check it against the upstream commit it ports:

```bash
python3 build/check-commit-ports.py <upstream-sha>
python3 build/check-commit-ports.py <upstream-sha> --pr <number>
```

The script lists the unit tests the commit adds or changes (with the catalog ids) and marks each one `COVERED` or `PARTIAL` (linked by `[UpstreamTest]`), `ACKNOWLEDGED` (named under a `Not ported` heading in the PR body), or `MISSING`. It exits with 1 when any test is `MISSING`. Tests in packages that are not ported, and in integration or e2e files, are listed apart. `--base` reads the links from the diff against that ref and does not read the PR body, so use `--pr` once the body is written. The default upstream clone is `/Users/dario/Code/vercel/ai`; pass `--upstream-repo` for another path.

Refresh the catalog when the compatibility commit moves:

```bash
git clone --filter=blob:none --sparse https://github.com/vercel/ai.git /tmp/vercel-ai
git -C /tmp/vercel-ai fetch --filter=blob:none origin <commit>
git -C /tmp/vercel-ai checkout <commit>
git -C /tmp/vercel-ai sparse-checkout set packages
python3 build/collect-upstream-tests.py /tmp/vercel-ai
python3 build/parity-report.py
```

Run `build/parity-report.py` before `build/open-upstream-test-issues.py`. The opener needs `generated/issues.json` and stops with an error if it is missing.
