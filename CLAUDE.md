# Porting an upstream change

- Port every test that the upstream commit adds or changes. Keep its name, steps, and assertions.
- A test that needs a real model or a testbed page is still a test to port. Port the page too. Run it as a real-model test (`[Category("RealModel")]`).
- When the port adds or changes public API or user-visible behavior, update `docs/` in the same PR. Mirror what upstream's `docs/**/*.mdx` say about that member, in the port's C# names.
- Skip an upstream test only when the port has no such feature at all (for example, tracing). Name each skipped test and the reason in the PR report.
- Lay out ported tests like upstream:
  - Each upstream test file is a directory, named in PascalCase. `protected-app-options.test.ts` becomes `ProtectedAppOptions/`.
  - Each `describe` in that file is one test class file inside the directory. `describe('web({ locale, timezoneId })')` becomes `ProtectedAppOptions/WebLocaleTimezoneIdTests.cs`.
  - Each `it` is one test method, named after the `it` text.
  - A class holds every `it` of its upstream `describe`, not only the ones the commit adds.
  - Tests outside any `describe` go in `<Directory>/<Directory>Tests.cs`.
  - The directory sits in the test project that runs the test, and the namespace follows it.
- Every test ports an upstream `it`. Do not keep a test that has no upstream `it`, even for .NET-only code. If an upstream `it` covers the behavior, port that `it` instead.
- A ported test checks everything its upstream `it` checks. If a part needs a feature the port does not have, name it in the PR report. Do not keep an upstream name on a test that checks only part of it.
- Out-of-scope work and follow-ups: whenever a port leaves something out (a skipped or partial test, a missing feature, a caveat you found), open a GitHub issue for it. Do this even when the PR report already names it. Search existing issues first to avoid duplicates. Link the issue from the PR report and reference the PR in the issue.
- Public types mirror upstream types. Never use `object`, `dynamic`, or `JsonElement` for a parameter or property that upstream types. Before you pick a C# type, read the upstream type and check how this port already exposes the same shape. For a `string | Model` union (such as `LanguageModel` or `DecisionModel`), use a typed model property plus a `ModelId` string property, as `Agent` and `AiClient` do. Copy the rule they use when both or neither is set. Do not copy an `object?` signature you find in older code (for example `Transcribe.cs`). It is a known deviation, not a precedent.
