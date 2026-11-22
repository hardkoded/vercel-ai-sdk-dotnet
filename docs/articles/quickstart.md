# Quickstart

Install `Vercel.AI` and the provider package you call. `OpenAIProvider.Create()` reads `OPENAI_API_KEY`. `GatewayProvider.Create()` reads `AI_GATEWAY_API_KEY`.

<!-- snippet: generate-text -->
<a id='snippet-generate-text'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(
    new GenerateTextOptions
    {
        ModelId = "openai/gpt-6-astra", // use Vercel AI Gateway
        Prompt = "What is an agent?",
    }
);

Console.WriteLine(result.Text);
```
<sup><a href='/examples/GenerateText/Program.cs#L14-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-generate-text' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`AiClient` is only required for the high-level helpers. Pass `Model` when you already have an `ILanguageModel`. Pass `ModelId` (`openai/gpt-4.1-mini`) when the Gateway should resolve it.

In ASP.NET Core, `services.AddAiSdk()` registers `IAiClient` and a named `HttpClient`.

## Model call callbacks

`GenerateTextOptions.OnStart` runs once when `GenerateTextAsync` begins, before any model call, and `OnStepStart` runs when each step begins, before the provider is called. Their `GenerateTextStartEvent` and `GenerateTextStepStartEvent` carry the call settings, including `ActiveTools` and `ToolOrder`. The step event also carries `StepNumber`, the `Steps` that finished before it, and the model, tool choice, tool order and provider options of that step after `PrepareStep`. `GenerateTextAsync` adds `ai/<version>` to the `user-agent` header, so `GenerateTextStartEvent.Headers` carries it. The telemetry settings (`Telemetry`, or the deprecated `ExperimentalTelemetry`) are not part of the event. A `ITelemetry` integration can implement `OnLanguageModelCallStart` to see the call settings of each model call. `StreamTextOptions` calls both too. `OnStepStart` runs once per step, also when a steering interrupt redoes the step's model call. An exception a callback throws is ignored.

`GenerateTextOptions.OnLanguageModelCallStart` runs immediately before each provider call begins, after the prompt is prepared. Its `LanguageModelCallStartEvent` carries `CallId`, `Provider`, `ModelId`, `Instructions`, `Messages`, `Tools` and `CallOptions` (the call settings). `CallId` is the same for every model call of one `GenerateTextAsync` or `StreamTextAsync` call, and the `LanguageModelCallEndEvent` carries the same `CallId`. `StreamTextOptions` calls it too, once for each step. An exception it throws is ignored.

`GenerateTextOptions.OnLanguageModelCallEnd` runs after each provider call returns, before tools run and before `OnStepEnd`. Its `LanguageModelCallEndEvent` carries `Provider`, `ModelId`, `FinishReason`, `Usage`, `Content`, `ResponseId`, `ProviderMetadata` and `ResponseTime`. It fires even when a tool then cancels the call, so the provider metadata of that call is not lost. `StreamTextOptions` calls it too, after the stream of a step ends.

`OnStepFinish` is the deprecated alias of `OnStepEnd` (maps to `onStepFinish`); `OnStepEnd` wins when both are set.

`GenerateObjectAsync<T>` is the deprecated typed-object counterpart to upstream `generateObject`; prefer `GenerateTextAsync` with `OutputSpec.Object`. It parses a provider-selected `StructuredOutputText` while its `OnStepEnd` callback still receives all raw text. Invalid JSON, schema mismatches, and empty responses throw `NoObjectGeneratedException`, which preserves the selected `Text`, `Cause`, `Response`, `Usage`, and `FinishReason`. `RepairText` can repair text that fails to parse or validate (`ExperimentalRepairText` is its deprecated alias; `RepairText` wins when both are set). `Telemetry` takes a `TelemetryOptions`; `ExperimentalTelemetry` is its deprecated alias.

Telemetry integrations: `GenerateTextOptions.Telemetry` (`TelemetryOptions`, with `ExperimentalTelemetry` as its deprecated alias) takes `Integrations`, a list of `ITelemetry`. An integration is upstream's `Telemetry` object, and its optional callbacks are properties that return `null` to skip. `OnEnd` and `OnLanguageModelCallEnd` are ported so far, and `GenerateTextAsync`, `StreamTextAsync`, and `StreamTextLowMemoryAsync` call them after the callbacks of the options. `TelemetryRegistry.RegisterTelemetry(...)` registers global integrations (upstream's `registerTelemetry` and `AI_SDK_TELEMETRY_INTEGRATIONS`), read by `GetGlobalTelemetryIntegrations()`. Per-call integrations replace the global ones, and `IsEnabled = false` turns both off. An enabled integration makes `StreamTextLowMemoryAsync` collect the content. Events sent to integrations carry `RecordInputs`, `RecordOutputs`, `FunctionId` and `AiVersion` (upstream's `augmentEvent`). `RecordInputs = false` removes the instructions, messages, tools and provider options from the model call start event and the request messages of the steps. `RecordOutputs = false` removes the content, text, reasoning, files, sources, tool calls and results, and response messages from the model call end event and the `OnEnd` result. When either flag is `false`, provider metadata and the request and response bodies are removed too. Otherwise the request and response bodies of the steps of `OnEnd` go through the provider's redactor (`LanguageModelRequestMetadata.RedactBody`, `StepResponse.RedactBody`). The other callbacks and the redaction of errors and tool errors are not ported yet.

`StreamObjectAsync<T>` is the deprecated typed-object counterpart to upstream `streamObject`; prefer `StreamTextAsync` with an `OutputSpec`. `StreamObjectOptions<T>.Output` selects `StreamObjectOutput.Object`, `Array`, `Enum` or `NoSchema`. For an array, `Schema` is the schema of one element and `T` is the array type. For an enum, set `EnumValues` instead of `Schema`. Combinations upstream rejects throw `InvalidArgumentException`. `AllowSystemInMessages` and `ExperimentalDownload` work as on `GenerateTextOptions`. The result streams `PartialObjectStream()` (partial objects, not validated), `ElementStream()` (complete array elements), `TextStream()`, and `FullStream()` (`ObjectStreamObjectPart`, `ObjectStreamTextDeltaPart`, `ObjectStreamErrorPart`, `ObjectStreamFinishPart`). `Object`, `Usage`, `FinishReason`, `Warnings`, `Request`, `Response` and `ProviderMetadata` are tasks that resolve when the response finishes. Errors never throw from the streams: they appear as error parts in `FullStream()` and go to `OnError`, and they fail the tasks. A final object that does not match the schema throws `NoObjectGeneratedException` from `Object`; `RepairText` can repair it (`ExperimentalRepairText` is its deprecated alias; `RepairText` wins when both are set). `Telemetry` takes a `TelemetryOptions` (`IsEnabled`, `RecordInputs`, `RecordOutputs`, `FunctionId`); `ExperimentalTelemetry` is its deprecated alias. Per-call integrations and `includeRuntimeContext` are not ported yet. Array output scans only the new text of each delta, so a long array is not parsed again for each delta. `OnStart` receives the full `GenerateObjectStartEvent` (`OperationId`, `System`, sampling settings, `MaxRetries`, `Headers`, `ProviderOptions`, `Output`, `Schema`, `SchemaName`, `SchemaDescription`), and `OnStepStart` also gets `ProviderOptions`, `Headers` and `PromptMessages` (`GenerateObjectAsync` fills the same events). `Request.Url` holds only the origin of the provider URL, without credentials, path, query or fragment. `ToTextStreamResponse()` and `PipeTextStreamToResponse()` write the text stream as `text/plain; charset=utf-8`.

`OnToolExecutionStart` runs before a tool's `Execute` and `OnToolExecutionEnd` after it returns or throws, on `GenerateTextOptions` and `StreamTextOptions`. They do not run for a tool without `Execute`, a provider-executed call, or an invalid or unapproved call. `ToolExecutionStartEvent` carries `CallId`, `ToolCall`, `ToolContext` (the validated context of the tool from `ToolsContext`) and `Messages` (what the model received in that step). `ToolExecutionEndEvent` adds `ToolOutput` (an `ExecutedTool`, with `IsError` set when the tool threw) and `ToolExecutionTime`. An exception a callback throws is ignored. The callbacks run before `OnStepEnd`.

## Timeouts

`GenerateTextOptions.Timeout` aborts the call when a time limit passes. It works with `GenerateTextAsync` and `StreamTextAsync`, and with the `CancellationToken` you pass: the call aborts when either one fires. Assign a `TimeSpan` for the total limit, or set the properties of a `TimeoutConfiguration`. Maps to `timeout`.

- `TotalMs`: the limit for the entire call, including all steps.
- `StepMs`: the limit for each step, including model generation, retries, and local tool execution. Each step starts a new budget.
- `FirstChunkMs`: the limit until the first chunk of model output of each step. It starts when the provider returns the response stream, so it does not limit the wait for the response itself (use `StepMs` for that). Each step starts a new budget. A chunk without output (response metadata, raw chunks, empty deltas) does not end it. `StreamTextAsync` only: `GenerateTextAsync` logs a warning for it.
- `ChunkMs`: the limit between chunks of model output, after the first chunk of output arrived. It does not limit the wait for the first chunk. A chunk without output (response metadata, raw chunks, empty deltas) does not reset it. The timer stops when the model response ends. `StreamTextAsync` only: `GenerateTextAsync` logs a warning for it.
- `ToolMs`: the limit for each tool execution. A tool that runs longer is cancelled through its `CancellationToken`, and its call ends with a tool error (an `ExecutedTool` with `IsError` set, and a `TimeoutError` as the `Error` when the tool stopped on its token), so the model can respond or retry. The limit starts after `OnToolExecutionStart`, and it cancels that tool only, not the call.
- `Tools`: per-tool limits that take precedence over `ToolMs`. Each key is the tool name followed by `Ms`, such as `weatherMs`. Upstream types the keys from the tool set. C# cannot, so a key that matches no tool has no effect. `TimeoutConfiguration.GetToolTimeoutMs(timeout, toolName)` returns the limit of one tool. Maps to `getToolTimeoutMs`.

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

var tools = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Prompt = "What is the weather in Lisbon?",
    Tools = new[] { weather, slowApi },
    Timeout = new TimeoutConfiguration
    {
        ToolMs = 5000, // default for all tools
        Tools = new Dictionary<string, int> { ["slowApiMs"] = 10000 }, // a longer limit for one tool
    },
});
```

`AgentOptions.Timeout` takes the same configuration for every `Agent.GenerateAsync` and `Agent.Stream` call. `PrepareCall` can replace it. Maps to `timeout` of `ToolLoopAgentSettings`. `Agent.GenerateAsync` and `Agent.Stream` also take a `timeout` argument for one call. It replaces both `AgentOptions.Timeout` and the timeout that `PrepareCall` returns.

`AgentOptions.Headers` sets extra HTTP headers for every request. `Agent` appends `ai-sdk-agent/tool-loop` to the `user-agent` header, keeping a caller-provided value, so usage can be attributed to the agent. Maps to `agentHeaders` of `ToolLoopAgent`.

When a limit passes, the `CancellationToken` of the model call and of every tool call is cancelled, and `LanguageModelCallOptions.AbortSignal` aborts with a `TimeoutError` reason. `GenerateTextAsync` throws that `TimeoutError`. The results of `StreamTextAsync` fault with it, as they do for any abort reason that is an exception. The model and the tools must observe the token to stop early. `AbortSignal` is `null` when the call has no timeout and no `StreamTextOptions.AbortSignal`.

Before this option worked, `Timeout` was a `TimeSpan?` that the call ignored. A `TimeSpan` still converts to a `TimeoutConfiguration`, so code that assigns a `TimeSpan` compiles and now takes effect. Code that assigns a `TimeSpan?` or reads `Timeout.Value` no longer compiles. `Timeout.InfiniteTimeSpan` means no timeout.
