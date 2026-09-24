# Vercel AI SDK for .NET

Unofficial community port of the [Vercel AI SDK](https://github.com/vercel/ai) for .NET. It is an independent reimplementation of the public AI SDK behavior and provider HTTP APIs. It is not a copy of Vercel’s TypeScript, not an official Vercel product, and not endorsed by Vercel.

The parity target is AI SDK 7 / Language Model specification V4. See [COMPATIBILITY.md](COMPATIBILITY.md) for the upstream commit and the packages that are intentionally not ported.

License: Apache License 2.0. Copyright 2023 Vercel, Inc.

[![NuGet](https://img.shields.io/nuget/v/Vercel.AI.svg)](https://www.nuget.org/packages/Vercel.AI)
**Docs:** https://hardkoded.github.io/vercel-ai-sdk-dotnet/

## Install

```bash
dotnet add package Vercel.AI
```

String model ids such as `openai/gpt-4.1-mini` are resolved by the Gateway, which ships inside this package.

## What you can call

`Vercel.AI` is a server library:

- `GenerateTextAsync` and `StreamTextAsync` (tools, structured JSON output, stop conditions, middleware)
- `EmbedAsync`, `EmbedManyAsync`, `RerankAsync`, `CosineSimilarity`
- `GenerateImageAsync`, `GenerateSpeechAsync`, `TranscribeAsync`, `TranslateAsync`, `GenerateVideoAsync`
- `Agent`, `ProviderRegistry`, and provider middleware
- Model providers, including the Vercel AI Gateway, in the same package

`ToUIMessageStreamResult` writes the AI SDK UI message stream (`text/event-stream`) that a JavaScript `useChat` client already consumes. That helper is available on `net10.0`. There is no React, Vue, Svelte, Angular, or RSC package in this repo.

String model ids such as `openai/gpt-4.1-mini` are resolved by the Gateway provider.

## Run the samples

```bash
dotnet run --project samples/Vercel.AI.Sample
dotnet run --project samples/Vercel.AI.AspNetCore.Sample
```

The console sample uses a scripted model unless `AI_GATEWAY_API_KEY` is set. The ASP.NET sample listens on `http://127.0.0.1:43123` and does the same for `POST /chat`.

```bash
curl -N -X POST http://127.0.0.1:43123/chat \
  -H 'content-type: application/json' \
  -d '{"prompt":"Hello"}'
```

## Tests

```bash
dotnet test Vercel.AI.slnx -c Release
```

Unit tests mock HTTP and do not need keys. Integration tests call a live provider only when `AI_GATEWAY_API_KEY` or `OPENAI_API_KEY` is set.

## Packages

The library targets `net10.0` and `netstandard2.0`. The ASP.NET UI stream helper and the samples are `net10.0` only. Versions come from MinVer. Pushing a `vMAJOR.MINOR.PATCH` tag packs `Vercel.AI`, publishes that package to nuget.org with Trusted Publishing, and the docs workflow publishes the DocFX site.

```csharp
var model = OpenAIProvider.Create().LanguageModel("gpt-4.1-mini");
var result = await new AiClient(Vercel.AI.Gateway.GatewayProvider.Create(new() { ApiKey = "unused" }))
    .GenerateTextAsync(new GenerateTextOptions
    {
        Model = model,
        Prompt = "Write a one-sentence release note.",
    });
Console.WriteLine(result.Text);
```

Dependency injection:

```csharp
services.AddAiSdk(options => options.GatewayApiKey = configuration["AI_GATEWAY_API_KEY"]);
services.AddOpenAI();
```

Provider keys use the same environment variables as the JavaScript providers (`OPENAI_API_KEY`, `ANTHROPIC_API_KEY`, `AI_GATEWAY_API_KEY`, and the rest listed in [COMPATIBILITY.md](COMPATIBILITY.md)).
