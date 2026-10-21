# Quickstart

Install `Vercel.AI` and the provider package you call. `OpenAIProvider.Create()` reads `OPENAI_API_KEY`. `GatewayProvider.Create()` reads `AI_GATEWAY_API_KEY`.

<!-- snippet: generate-text -->
<a id='snippet-generate-text'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra", // use Vercel AI Gateway
    Prompt = "What is an agent?",
});

Console.WriteLine(result.Text);
```
<sup><a href='/examples/GenerateText/Program.cs#L14-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-generate-text' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`AiClient` is only required for the high-level helpers. Pass `Model` when you already have an `ILanguageModel`. Pass `ModelId` (`openai/gpt-4.1-mini`) when the Gateway should resolve it.

In ASP.NET Core, `services.AddAiSdk()` registers `IAiClient` and a named `HttpClient`.

## Model call callbacks

`GenerateTextOptions.OnLanguageModelCallEnd` runs after each provider call returns, before tools run and before `OnStepEnd`. Its `LanguageModelCallEndEvent` carries `Provider`, `ModelId`, `FinishReason`, `Usage`, `Content`, `ResponseId`, `ProviderMetadata` and `ResponseTime`. It fires even when a tool then cancels the call, so the provider metadata of that call is not lost. `StreamTextOptions` calls it too, after the stream of a step ends.

`OnToolExecutionStart` runs before a tool's `Execute` and `OnToolExecutionEnd` after it returns or throws, on `GenerateTextOptions` and `StreamTextOptions`. They do not run for a tool without `Execute`, a provider-executed call, or an invalid or unapproved call. `ToolExecutionStartEvent` carries `ToolCall` and `Messages` (what the model received in that step). `ToolExecutionEndEvent` adds `ToolOutput` (an `ExecutedTool`, with `IsError` set when the tool threw) and `ToolExecutionTime`. An exception a callback throws is ignored. The callbacks run before `OnStepEnd`.

## Timeouts

`GenerateTextOptions.Timeout` aborts the call when a time limit passes. It works with `GenerateTextAsync` and `StreamTextAsync`, and with the `CancellationToken` you pass: the call aborts when either one fires. Assign a `TimeSpan` for the total limit, or set the properties of a `TimeoutConfiguration`. Maps to `timeout`.

- `TotalMs`: the limit for the entire call, including all steps.
- `StepMs`: the limit for each step, including model generation, retries, and local tool execution. Each step starts a new budget.
- `FirstChunkMs`: the limit until the first chunk of model output of each step. It starts when the model response of the step starts, and each step starts a new budget. A chunk without output (response metadata, raw chunks, empty deltas) does not end it. `StreamTextAsync` only: `GenerateTextAsync` logs a warning for it.
- `ChunkMs`: the limit between chunks of model output, after the first chunk of output arrived. It does not limit the wait for the first chunk. A chunk without output (response metadata, raw chunks, empty deltas) does not reset it. The timer stops when the model response ends. `StreamTextAsync` only: `GenerateTextAsync` logs a warning for it.

```csharp
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Prompt = "Invent a new holiday and describe its traditions.",
    Timeout = new TimeoutConfiguration { TotalMs = 60000, StepMs = 10000 },
});

var stream = client.StreamTextAsync(new StreamTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Prompt = "Invent a new holiday and describe its traditions.",
    Timeout = new TimeoutConfiguration { ChunkMs = 5000 }, // abort if the output stalls for 5 seconds
});
```

When a limit passes, the `CancellationToken` of the model call and of every tool call is cancelled, and `LanguageModelCallOptions.AbortSignal` aborts with a `TimeoutError` reason. `GenerateTextAsync` throws that `TimeoutError`. The results of `StreamTextAsync` fault with it, as they do for any abort reason that is an exception. The model and the tools must observe the token to stop early. `AbortSignal` is `null` when the call has no timeout and no `StreamTextOptions.AbortSignal`.

Before this option worked, `Timeout` was a `TimeSpan?` that the call ignored. A `TimeSpan` still converts to a `TimeoutConfiguration`, so code that assigns a `TimeSpan` compiles and now takes effect. Code that assigns a `TimeSpan?` or reads `Timeout.Value` no longer compiles. `Timeout.InfiniteTimeSpan` means no timeout. `toolMs` and per-tool timeouts of upstream are not available yet. See [#702](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/702).
