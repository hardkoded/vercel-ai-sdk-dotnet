# Contributing

This is an independent reimplementation of the public Vercel AI SDK. Do not copy Vercel’s TypeScript into this repository. Match the public HTTP APIs and the V4 specification, and keep the Apache-2.0 header on every C# file:

```
// Copyright 2023 Vercel, Inc.
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

Warnings are errors. `dotnet test Vercel.AI.Sdk.slnx -c Release` is the check that has to pass.

Provider tests should mock `HttpMessageHandler` and assert the request URL and authentication header. Live calls belong in `[SkippableFact]` tests that skip when the provider’s environment variable is empty.

When the upstream commit in `COMPATIBILITY.md` moves, update that file and the provider notes in the same change.
