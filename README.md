# Vercel AI SDK for .NET

Unofficial community port of the [Vercel AI SDK](https://github.com/vercel/ai) for .NET. It is an independent reimplementation of the public AI SDK behavior and provider HTTP APIs. It is not a copy of Vercel’s TypeScript, not an official Vercel product, and not endorsed by Vercel.

The parity target is AI SDK 7 / Language Model specification V4. See [COMPATIBILITY.md](COMPATIBILITY.md) for the upstream commit and the packages that are intentionally not ported.

License: Apache License 2.0. Copyright 2023 Vercel, Inc.

## What you can call

`Vercel.AI.Sdk` is a server library:

- `GenerateTextAsync` and `StreamTextAsync` (tools, structured JSON output, stop conditions, middleware)
- `EmbedAsync`, `EmbedManyAsync`, `RerankAsync`, `CosineSimilarity`
- `GenerateImageAsync`, `GenerateSpeechAsync`, `TranscribeAsync`, `TranslateAsync`, `GenerateVideoAsync`
- `Agent`, `ProviderRegistry`, and provider middleware
- One package per model provider, including the Vercel AI Gateway

`Vercel.AI.Sdk.AspNetCore` writes the AI SDK UI message stream (`text/event-stream`) that a JavaScript `useChat` client already consumes. There is no React, Vue, Svelte, Angular, or RSC package in this repo.

String model ids such as `openai/gpt-4.1-mini` are resolved by the Gateway provider.

## Run the samples

```bash
dotnet run --project samples/Vercel.AI.Sdk.Sample
dotnet run --project samples/Vercel.AI.Sdk.AspNetCore.Sample
```

The console sample uses a scripted model unless `AI_GATEWAY_API_KEY` is set. The ASP.NET sample listens on `http://127.0.0.1:43123` and does the same for `POST /chat`.

```bash
curl -N -X POST http://127.0.0.1:43123/chat \
  -H 'content-type: application/json' \
  -d '{"prompt":"Hello"}'
```

## Tests

```bash
dotnet test Vercel.AI.Sdk.slnx -c Release
```

Unit tests mock HTTP and do not need keys. Integration tests call a live provider only when `AI_GATEWAY_API_KEY` or `OPENAI_API_KEY` is set.

## Packages

Projects target `net10.0` and `netstandard2.0`, except `Vercel.AI.Sdk.AspNetCore` and the samples, which are `net10.0` only. Versions come from MinVer. Tag a release as `vMAJOR.MINOR.PATCH`. This repository does not publish to nuget.org on its own.

```csharp
var model = OpenAIProvider.Create().LanguageModel("gpt-4.1-mini");
var result = await new AiClient(Vercel.AI.Sdk.Gateway.GatewayProvider.Create(new() { ApiKey = "unused" }))
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
