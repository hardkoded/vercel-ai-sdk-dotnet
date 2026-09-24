---
name: vercel-ai-dotnet
description: Use the community .NET port of the Vercel AI SDK. Covers GenerateTextAsync, StreamTextAsync, tools, structured output, providers, and the ASP.NET Core UI message stream.
---

# Vercel AI SDK for .NET

Unofficial port of vercel/ai. Apache-2.0. Not an official Vercel product. Parity target is Language Model specification V4.

## Map JavaScript names to C#

- `generateText` → `GenerateTextAsync`
- `streamText` → `StreamTextAsync` (returns `StreamTextResult`)
- `AbortSignal` → `CancellationToken`
- `tool()` → `Tool.Function(name, description, jsonSchema, execute)`
- `stopWhen: isStepCount(n)` → `StopWhen.IsStepCount(n)`
- `"openai/gpt-4.1-mini"` → `GenerateTextOptions.ModelId` (Gateway) or a provider’s `LanguageModel`

## Generate text

```csharp
var model = OpenAIProvider.Create().LanguageModel("gpt-4.1-mini");
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "Summarize this incident in one sentence.",
    Tools = new[]
    {
        Tool.Function("lookup", "Lookup a host", "{\"type\":\"object\"}", (args, ct) => Task.FromResult("{\"ok\":true}")),
    },
    StopWhen = StopWhen.IsStepCount(4),
    Output = OutputSpec.Object("{\"type\":\"object\"}", "report"),
});
```

`result.Text` is the last step. `result.Output` is the parsed JSON object when `Output` is set. The default stop condition is one model step; tools from that step still execute.

## Stream

`StreamTextResult.TextStream` yields text deltas. `Stream` yields every part. In ASP.NET Core, `result.ToUIMessageStreamResult()` writes `text/event-stream` for a JavaScript `useChat` client. Do not add a React or Razor chat component for that protocol.

## Providers

Each package exposes `CreateXxx()` / `AddXxx()` and reads the same environment variable as the JavaScript provider. Gateway uses `AI_GATEWAY_API_KEY` and `https://ai-gateway.vercel.sh/v4/ai`. OpenAI-compatible providers share `Vercel.AI.OpenAICompatible`.

Register the client with `services.AddAiSdk()`. Add `services.AddAiSdkOpenTelemetry()` to record spans on the `Vercel.AI` activity source.

## Tests

Mock `HttpMessageHandler`. Use `TestLanguageModel` from `Vercel.AI.Testing` when the test does not care about HTTP. Skip live tests unless the provider key is set.
