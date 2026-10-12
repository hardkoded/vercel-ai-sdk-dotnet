# Contributing

This is an independent reimplementation of the public Vercel AI SDK. Do not copy Vercel’s TypeScript into this repository. Match the public HTTP APIs and the V4 specification, and keep the Apache-2.0 header on every C# file:

```
// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0
```

## JavaScript to C#

| JavaScript | C# |
| --- | --- |
| `generateText` | `GenerateTextAsync` |
| `streamText` | `StreamTextAsync`, returning `StreamTextResult` |
| `embed` / `embedMany` | `EmbedAsync` / `EmbedManyAsync` |
| `AbortSignal` | `CancellationToken` |
| `camelCase` options | PascalCase properties |
| thrown errors | `AiSdkException` and the `ApiException` subclasses |
| `tool()` | `Tool.Function` |
| `stopWhen: isStepCount(1)` | `StopWhen.IsStepCount(1)` |
| `wrapLanguageModel` | `WrapLanguageModel` |
| string model id | `GenerateTextOptions.ModelId`, resolved by Gateway |

Projects multi-target `net10.0` and `netstandard2.0` unless they depend on ASP.NET Core. Avoid APIs that are missing on `netstandard2.0`, including `string.StartsWith(string, StringComparison)`, `Random.Shared`, and `SHA256.HashData`.

Warnings are errors. `dotnet test Vercel.AI.slnx -c Release` is the check that has to pass.

Provider tests should mock `HttpMessageHandler` and assert the request URL and authentication header. Live calls belong in `[SkippableFact]` tests that skip when the provider’s environment variable is empty.

When the upstream commit in `COMPATIBILITY.md` moves, update that file and the provider notes in the same change.

## Examples

Every code block in `README.md` comes from a runnable project in `examples/`. Do not type code into the README by hand. Use the same way for new code in `docs/articles`. Moving its older hand-written blocks is tracked in #250. [MarkdownSnippets.MsBuild](https://github.com/SimonCropp/MarkdownSnippets) rewrites the markdown when `examples/GenerateText` builds, so the code cannot drift from the API. CI fails when the committed markdown does not match the build output.

To add an example:

1. Create `examples/<Name>/<Name>.csproj` with `Microsoft.NET.Sdk` and a `Description`. `examples/Directory.Build.props` makes it a `net10.0` console app that references `Vercel.AI`.
2. Write `Program.cs` with the C# file header. Call a real model when `AI_GATEWAY_API_KEY` (or the provider's key) is set. When the key is missing, print which variable to set and exit.
3. Put the code to show between `#region name` and `#endregion`. Region names must be unique across the repository.
4. Add the project to `Vercel.AI.slnx`.
5. Reference the snippet in a markdown file with `<!-- snippet: name -->` and `<!-- endSnippet -->`.
6. Run `dotnet build Vercel.AI.slnx` and commit the markdown it rewrites. Check that `dotnet run --project examples/<Name>` works.
