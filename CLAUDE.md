# Porting an upstream change

- Port every test that the upstream commit adds or changes. Keep its name, steps, and assertions.
- Before you report a port as ready, run `python3 build/check-commit-ports.py <upstream-sha> --pr <n>`. It must show no MISSING or AMBIGUOUS test. Every test it lists as ACKNOWLEDGED must be named under "Not ported" in the PR body. A changed test that already had a link needs a manual look.
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


# Reviewing .NET and C# code

Run the `dotnet-claude-kit:code-review` skill on every C# change. Also check these points. Upstream parity, the last but one point, wins over the others.

- **Cleanup on failure.** Dispose every `HttpResponseMessage`, stream, reader, enumerator, and `CancellationTokenSource` when a later step throws or the caller stops early. Look for a resource created before a call that can throw, with no `using`, `await using`, or `finally`. Put all disposals in a `finally` so one throw does not skip the next.
- **Streaming.** Do not do synchronous I/O in an async path. Do not dispose a `StreamWriter` or `StreamReader` over a response stream with a synchronous `Dispose` when the host forbids it (Kestrel does). Stop reading the provider response when the consumer stops.
- **Cancellation.** Pass the caller's `CancellationToken` down to every awaited call. Mark an `IAsyncEnumerable` token parameter with `[EnumeratorCancellation]`. A caller cancel must surface as cancellation. Do not catch `OperationCanceledException` to return `null`, an empty result, or a provider error.
- **Waits and caches.** Every network or process wait needs a bound. Do not cache a failed `Lazy<Task>` or a faulted `Task`. Write files through a unique temp name, then move.
- **Secrets.** Do not put an API key, a bearer token, or a signed header into an exception message, a warning, provider metadata, or a log line. Redact before you copy a request or response into an error. Do not follow a URL from outside text without checking the scheme.
- **Shared state.** A provider or model instance is shared across calls. Keep per-call state in the call, not in a field. Use `Interlocked` or a lock for a counter or a cache that concurrent calls touch. Do not add a mutable `static`.
- **Regex.** Use `\z`, not `$`, when a whole-string match must reject a trailing newline. Give a regex that matches outside text a match timeout.
- **JSON and culture.** Use `CultureInfo.InvariantCulture` when you format or parse a number or date for a wire format. Do not swallow a `JsonException` to hide a bad provider response. Report it as an error.
- **Compatibility.** Code in `src/` must build for `netstandard2.0` and `net10.0`. Use a polyfill from `src/Polyfill` when a newer API is missing.
- **Upstream parity.** Check each fix against upstream first. If upstream behaves the same on purpose, report it. Do not "fix" it in the port.
- **Modern C#.** Match the file you edit. Use collection expressions, primary constructors, and `field` in new code where `netstandard2.0` allows. Do not rewrite untouched code to modernize it.
