# Streaming

`StreamTextAsync` returns immediately. Enumerate `TextStream` for text deltas, or `Stream` for tool calls, tool results, sources, and the finish part.

<!-- snippet: streaming -->
<a id='snippet-streaming'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var stream = client.StreamTextAsync(
    new StreamTextOptions { ModelId = "openai/gpt-6-astra", Prompt = "Draft a changelog entry." }
);

await foreach (var delta in stream.TextStream())
{
    Console.Write(delta);
}

Console.WriteLine();
Console.WriteLine(await stream.FinishReason);
```
<sup><a href='/examples/StreamText/Program.cs#L14-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-streaming' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Partial output stream

Set `Output` and enumerate `PartialOutputStream` for the structured output as it grows. Each value is a `JsonElement`. With `OutputSpec.Object`, it is the partial object, repaired from the JSON text so far. With `OutputSpec.Array`, it is the array of elements that are complete so far, and the repaired last element is left out. With `OutputSpec.Choice`, it is the option, published only when it is not ambiguous. With `OutputSpec.Json`, it is the JSON parsed so far. With text output (no `Output`, or `OutputSpec.Text()`), it is the text so far as a JSON string. A value is published only when it differs from the last one, each step starts over, `TextStream()` and the `TextDeltaPart` of a structured output carry text that is held back until the partial output changes (for `OutputSpec.Object`, a delta that ends after a key joins the next one), `OutputSpec.Array` scans only the new text of each delta, and text that the model streams in a later text block of a step is ignored. The partial object and the partial elements are not validated against the schema, except that an element of a partial array that does not match the element schema is left out. Maps to `partialOutputStream`. `ExperimentalPartialOutputStream()` is the deprecated alias (upstream `experimental_partialOutputStream`).

For `OutputSpec.Array`, enumerate `ElementStream()` to get each element once, as soon as it is complete. The stream fails with a `TypeValidationException` when the model sends more elements than `maxItems`. It is empty for any other output. Maps to `elementStream`.

`GenerateTextResult.Output` is the text as a JSON string when no JSON output is set (no `Output`, or `OutputSpec.Text()`), for a stop response or a non-empty response that is not a tool call. It is null otherwise. Maps to `output ?? text()`.

`StreamTextResult.Output` resolves to the parsed output: the object, the array of elements, the choice option, the JSON value, or the text as a JSON string for text output. An answer that does not parse fails `Output` with a `NoObjectGeneratedException` and leaves the stream and the other results alone.

`OutputSpec.Array(elementSchema, minItems, maxItems)` asks the model for `{ "elements": [...] }`, `OutputSpec.Choice(options)` for `{ "result": option }`, and `OutputSpec.Json()` for any JSON value. They map to `Output.array`, `Output.choice`, and `Output.json`.

```csharp
await foreach (var partialObject in result.PartialOutputStream())
{
    Console.WriteLine(partialObject);
}
```

## Single-consumer stream

`Ai.StreamTextLowMemoryAsync(options)` (or `AiClient.StreamTextLowMemoryAsync`) takes the same options as `StreamTextAsync` and returns a `StreamTextSingleConsumerResult`. It has no replay: a part is released once the consumer reads it, and the result serves one stream projection (`Stream()`, `TextStream()`, `PartialOutputStream()`, `ElementStream()`, or `ConsumeStreamAsync()`). A second projection throws an `AiSdkException` ("This result supports one stream consumer"). `Usage` and `FinishReason` can be awaited at any time. `PartialOutputStream()` without an explicit `Output` throws `UnsupportedFunctionalityException`. Maps to `streamTextLowMemory`.

```csharp
var result = Ai.StreamTextLowMemoryAsync(new StreamTextOptions { Model = model, Prompt = "Hi" });
await foreach (var delta in result.TextStream()) { Console.Write(delta); }
```

## UI message stream

`Vercel.AI.AspNetCore` writes the same Server-Sent Events a JavaScript `useChat` client reads:

```csharp
app.MapPost("/chat", (ChatRequest request) =>
{
    var stream = client.StreamTextAsync(new StreamTextOptions { Model = model, Prompt = request.Prompt });
    return stream.ToUIMessageStreamResult(keepAliveMs: 15_000);
});
```

The response is `text/event-stream` with `x-vercel-ai-ui-message-stream: v1`. Chunks include text, reasoning, tool input and output, sources, step boundaries, finish, and errors. See COMPATIBILITY.md for chunk types that are not emitted yet.

`keepAliveMs` is optional. When it is set, the response writes `: stream-open` before the start event and `: keep-alive` whenever the next part is still pending after that many milliseconds. Omit it, or pass null, and the stream contains no comment lines.

## A stream that ends without output

When a provider stream ends without a finish part and without any output (no text, reasoning, tool input, or tool call), `StreamTextAsync` does not record an empty step. It reports a `NoOutputGeneratedException` (`AI_NoOutputGeneratedError`) with the message `No output generated. The model stream ended without a finish chunk.` `OnError` receives it, the stream ends with an `ErrorPart` (its `Error` is the `NoOutputGeneratedException`) and then completes without throwing, and `Text`, `Steps`, `FinishReason`, and `Usage` fault with it. `OnStepEnd` is not called for that step, but steps that finished before it keep their callbacks. When a later step of a tool loop ends this way, the whole call fails.

A stream that ends without a finish part after it produced output keeps the partial result. The step finishes with `FinishReason.Other` and `OnError` is not called.

When `DoStreamAsync` fails before returning a provider stream, `StreamTextAsync` emits an `ErrorPart`, calls `OnError`, and completes its text and full-part streams without throwing from enumeration. If no earlier step completed, `Text` rejects with `NoOutputGeneratedException` and the message `No output generated. Check the stream for errors.` An `ErrorStreamPart` received during a provider stream also calls `OnError`; a following finish part still completes the step and calls `OnEnd`.

The `GenerateTextResult` that `OnEnd` receives also exposes `CallId`, `StepNumber`, `Provider`, `ToolsContext`, `RuntimeContext`, `RawFinishReason`, `TotalUsage` (same as `Usage`), and the `StaticToolCalls`, `DynamicToolCalls`, `StaticToolResults`, and `DynamicToolResults` of every step. Each `StepResult` carries `CallId`, `StepNumber`, `ToolsContext`, `RuntimeContext`, and the same static/dynamic splits.

## Cancel a stream

Cancel the `CancellationToken` you passed to `StreamTextAsync`, or set `StreamTextOptions.AbortSignal` to cancel with a reason. The call rejects `Text`, `Steps`, `FinishReason`, and `Usage` at once. It does not wait for your callbacks or for the provider to close the response. A reason that is an exception is the exception the tasks fault with. A token faults them with an `OperationCanceledException`.

The stream ends with an `AbortPart` after `OnAbort` returns. `OnAbort` receives the steps that finished before the cancel and the exception. `OnError` and `OnEnd` are not called.

```csharp
var controller = new AbortController();
var stream = client.StreamTextAsync(new StreamTextOptions
{
    Model = model,
    Prompt = "Draft a changelog entry.",
    AbortSignal = controller.Signal,
    OnAbort = (context, _) =>
    {
        Console.WriteLine($"Aborted after {context.Steps.Count} steps.");
        return Task.CompletedTask;
    },
});

controller.Abort(new JsError("manual abort"));
```

`Agent.Stream` takes the same `abortSignal` argument. Set `AgentOptions.OnAbort` to receive the abort.

## Sources and citations

`Sources` on a step or a `GenerateTextResult` is the retrieved reference set: the URLs a search returned, which can include more references than the model cites in its answer. Inline references hang off the text they support instead. `GeneratedText.Citations` is an optional list of `Citation` values on each text part in `StepResult.Content` (or `GenerateTextResult.Content`, which spans every step). Use `Citations` to render inline references and `Sources` to list what was retrieved.

A `Citation` has:

- `Source`: a `GeneratedSource` (a URL, with its title when the provider sent one) or a `GeneratedDocumentSource` (an `Id`, `MediaType`, `Title`, optional `Filename`, and `ProviderMetadata`). Its id identifies the cited resource. It does not have to match an id in `Sources`.
- `StartIndex` and `EndIndex`: optional provider-supplied offsets in the containing text. The end is exclusive. The SDK keeps the offsets as the provider sent them, so check the provider's indexing convention before slicing a .NET string, which is indexed by UTF-16 code unit. When offsets are missing, show the references next to the whole text block.
- `CitedText`: an optional supporting passage from the source. It is source text, not a position in the answer.

Preserve query strings and fragments in source URLs. They can identify a version, page, or passage, and several citations can point at one source at different places in the text.

```csharp
var result = await client.GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "Summarize the news." });
foreach (var part in result.Content.OfType<GeneratedText>())
{
    foreach (var citation in part.Citations ?? Array.Empty<Citation>())
    {
        Console.WriteLine($"{citation.Source} {citation.StartIndex}-{citation.EndIndex}");
    }
}
```

In a model stream, citations arrive on `TextEndStreamPart.Citations`, tied to the id of the text block they end. `StreamTextAsync` keeps them on the matching text part in `StepResult.Content`. `SimulateStreamingMiddleware` copies `GeneratedText.Citations` onto the text-end part it emits, and emits a text block that has citations even when its text is empty. Citation-derived sources can arrive at the end of the provider stream, after the SDK knows whether the provider sent retrieval data.

### Responding with a UI message stream

`UIMessageStreamResponses.CreateUIMessageStreamResponse(stream, init)` is the counterpart of upstream `createUIMessageStreamResponse`. It encodes a `ReadableStream<JsonObject>` of UI message chunks as Server-Sent Events and returns a `UIMessageStreamResponse` with `Status`, `StatusText`, `Headers`, and a UTF-8 `Body`. Use `ReadableStream<JsonObject>.From(chunks)` to turn the `IAsyncEnumerable` of `UIMessageStreamConverter.ToUIMessageStream` into a stream. `UIMessageStreamResponseInit` takes `Status`, `StatusText`, `Headers`, `KeepAliveMs`, and `ConsumeSseStream`.

`ConsumeSseStream` receives a `ReadableStream<string>` copy of the SSE stream (made with `ReadableStream<T>.Tee()`) and does not block the response. Pass `s => ConsumeStream.ConsumeAsync(s)` to keep the source flowing when the client disconnects. A consumer that stops reading cancels the stream, so `OnEnd` reports `IsCancelled` with an `Unknown` outcome. With a consumer that keeps reading, the source sees an abort and `OnEnd` reports `IsAborted` with an `Aborted` outcome. An abort that follows the consumer's cancellation right away (within one second) still calls `OnAbort`, because the run ends asynchronously after the consumer stops.

### Error text in the UI message stream

`PipeUIMessageStreamToResponseAsync(ServerResponse response, status, statusText, headers, keepAliveMs, onError, redactToolErrors, sendSources)` is the counterpart of upstream `pipeUIMessageStreamToResponse`. It writes the same SSE stream to a `ServerResponse` byte sink and ends it. Headers you pass take precedence over the protocol headers.

By default, the client receives the same error text for a failed tool call as the model. This applies to tool execution errors and to invalid tool calls of tools that are not provider-executed. The UI messages are sent back to the model on the next request, so they keep the text that the model's reasoning and answers are based on. A stream `error` chunk never reaches the model, so `ToUIMessageStreamResult` and `UIMessageStreamResult` mask it with `An error occurred.` unless you pass `onError`.

The error text of a failed tool call can contain details from your server, such as host names or file paths. The model receives this text and can repeat it in its answer. To keep such details away from the model and the client, catch the error in the tool's `execute` function and throw an exception with a safe message. The `execute` function does not run for an invalid tool call. Its error text contains the input that the model generated, the validation messages of the input schema, or the names of the available tools. Only `redactToolErrors` replaces this text. Errors of provider-executed tools are never redacted. Invalid tool calls of provider-executed tools always use `onError`.

Pass `redactToolErrors: true` to replace the error text of failed tool calls with the result of `onError`, which returns `An error occurred.` by default. `redactToolErrors` defaults to `true` when you pass `onError` and to `false` otherwise, so an existing `onError` keeps handling tool errors. Pass `redactToolErrors: false` to limit your `onError` to stream errors. The function receives the error (an `Exception`), as upstream's `onError` does. A stream `ErrorPart` carries it in its `Error` property.

Redacting tool errors changes the message history. The tool result that `ConvertToModelMessages` creates from the stored UI messages on the next request is then not the tool result that the model received. Some models bind their reasoning to the messages before it and reject a request when those messages have changed. For example, Anthropic models with preserved thinking respond with a 400 error, and every later request with these messages fails the same way. The writer logs a warning, once per stream, when it sent a tool error text that differs from the text the model received and then sent reasoning from a later step. See [Warnings](warnings.md). If you redact tool errors, use an Anthropic adaptive-thinking `blockBinding.prefixMismatchBehavior` of `drop_block`, or leave reasoning out of the UI messages.

### Converting stream parts to UI message chunks

`UIMessageStreamConverter.ToUIMessageChunk(part, options)` converts one `TextStreamPart` into a UI message chunk (a `JsonObject`), or returns `null` for a part that produces no chunk. `UIMessageStreamConverter.ToUIMessageStream(parts, options)` converts a whole stream of parts, adds the response message id, and logs the tool error text mismatch warning. `ToUIMessageStreamResult` uses it. `ToUIMessageChunk` takes a `ToUIMessageChunkOptions` with `Tools` (a call, input, result, or error of a dynamic tool is marked as dynamic), `SendReasoning` (default `true`), `SendSources` (default `false`: source parts are skipped), `SendStart`, `SendFinish`, `OnError`, `RedactToolErrors`, `MessageMetadata` (a value, sent with the `start` and `finish` chunks), and `ResponseMessageId`. `ToUIMessageStream` takes a `UIMessageStreamOptions` with `Tools`, `SendReasoning`, `SendSources`, `SendStart`, `SendFinish`, `OnError`, `RedactToolErrors`, `MessageMetadata` (a function called for each part; its value goes with the `start` and `finish` chunks and in a separate `message-metadata` chunk for any other part), `OriginalMessages`, `GenerateMessageId`, `OnStepEnd`, and `OnEnd`. The `start` chunk has a `messageId` only when `GenerateMessageId` is set (or the last original message is an assistant message and you pass `GenerateMessageId`). Use them to convert parts that you built or changed yourself.

`ToUIMessageChunk` maps every `TextStreamPart`. `CustomPart` becomes a `custom` chunk, `SteeringMessagePart` a `steering-message` chunk whose parts are the text and file parts of its user message, and `DocumentSourcePart` a `source-document` chunk (sent only with `SendSources`). `ToolApprovalRequestPart` keeps its `Reason`, `IsAutomatic`, and `Signature`, and adds `inputSchemaInput` when `GeneratedToolCall.InputSchemaInput` differs from the call's input. `ToolApprovalResponsePart` and `ToolOutputDeniedPart` map to their chunks. A tool result with no output is sent as `null`. `streamText` decides the approval of each call as it arrives, so its approval request, response, and denial parts follow their tool call part, and the step content keeps the stream order. It emits the approval request, response, and denial parts of a call that an automatic decision settled, the denial, error, and result parts of the approval responses that end the history (see [tools](tools.md)), and the `custom` and document source parts that a model streams. A part of an unknown type throws `InvalidOperationException`.

`OnStepEnd` runs when a step ends and `OnEnd` runs once when the stream ends. Both receive the updated `Messages`, `IsContinuation`, and the `ResponseMessage`. When the last of `OriginalMessages` is an assistant message, the response continues it and reuses its id. `OnEnd` also receives `IsAborted`, `FinishReason`, and an `Outcome`: `Completed`, `Failed` (with the `Error`), `Aborted`, or `Unknown`. A failure of the source stream or of the conversion, such as an `OnError` that throws, always reports `Failed`, and it reports it once. A consumer that stops reading before the stream declares an outcome keeps `Unknown` and sets `IsCancelled`.

## Validating UI messages

`Ai.ValidateUIMessagesAsync(new ValidateUIMessagesOptions { Messages = messages })` checks the UI messages that you load from storage or receive from a client before you convert them with `ConvertToModelMessages`. `Ai.SafeValidateUIMessagesAsync` returns a `SafeValidateUIMessagesResult` (`Success`, `Data`, or `Error`) instead of throwing. A null `Messages` throws `InvalidArgumentException`. An empty list, and a system or user message with no parts, throw `TypeValidationException`. An assistant message can have no parts, as the last message of a chat that waits for a response. Pass `MessagesJson` (a `JsonElement`, such as a request body) instead of `Messages` to validate untyped input. Set either one, not both. The structure of the JSON is checked first: the role, the parts, each part type and tool state, the fields of each part, and the JSON values of provider metadata and tool metadata (a number must be finite, and `__proto__` keys are removed). Each array stops at its first invalid element, and each part is checked only against the schema of its type, so the size of the error does not grow with the input. The failure is a `TypeValidationException` whose message lists the issue paths. On success the result holds typed messages. A typed `Messages` list needs no structural check. The options validate the values:

- `MetadataSchema`: a JSON Schema for `UIMessage.Metadata`.
- `DataSchemas`: JSON Schemas of the data parts, by data name. A data part without a schema is an error.
- `Tools`: the tools of the tool parts, by name. The input of each tool part is validated with `Tool.InputSchema`, and the output of a finished call with `Tool.OutputSchema`. A tool part with no tool is an error, unless its call has finished. Then the part becomes a `dynamic-tool` part whose `Dynamic` is `false`, and `ConvertToModelMessages` replaces its output with `Tool output omitted because the tool is no longer available.`. A failed call, or a finished call with an empty input, that does not match the input schema is converted the same way.
- `ExperimentalRefineToolInput`: rebuilds the input of a tool call from `ToolUIApproval.InputSchemaInput`, by tool name, when the input schema changes the value. The validated input is compared with the input of the part, and a mismatch is an error.

The validated messages are new objects. The messages that you pass in are not changed.

## Reading UI message streams

`UIMessageStreamConverter.ReadUIMessageStream(new ReadUIMessageStreamOptions(chunks))` turns a stream of UI message chunks into an `IAsyncEnumerable<UIMessage>`. Each element is another state of the same message while it is being built. Set `Message` to continue the last assistant message when you resume a conversation. A chunk that starts a different message id replaces it. `OnError` receives errors, including `error` chunks, and `TerminateOnError` ends the enumeration with the error instead. Stopping the enumeration early disposes the chunk stream. The typed `UIMessage` keeps the fields that the chunks carry: `TextUIPart.Citations`, `ToolUIPart.InputEncoding` (while the call is `InputStreaming`, an input with `Text` encoding stays a raw string), `ToolUIPart.ToolMetadata`, and `SteeringMessageUIPart` (`steering-message`).

```csharp
var chunks = UIMessageStreamConverter.ToUIMessageStream(result.Stream());

await foreach (var message in UIMessageStreamConverter.ReadUIMessageStream(new ReadUIMessageStreamOptions(chunks)))
{
    Console.WriteLine(message.Parts.Count);
}
```

`ToolInputStartPart.InputEncoding` (from `ToolInputStartStreamPart.InputEncoding`) is `ToolInputEncoding.Json` or `ToolInputEncoding.Text`. It says how to read the `tool-input-delta` parts that follow. When it is `null` or `Json`, the deltas concatenate to partial JSON. With `Text`, they concatenate to the raw string input, such as code or HTML from a custom tool. The final tool call input stays JSON-encoded either way. `UIMessageStream` writes it as the `inputEncoding` field (`"json"` or `"text"`) of the `tool-input-start` chunk and leaves the field out when it is `null`.

## Full stream parts

`result.Stream()` yields these parts in order: one `StartPart` (`start`), then for each step a `StartStepPart` (`start-step`, with the step's `Warnings` and `Request`), the step's text, reasoning, tool-input, tool-call, source and tool parts, and a `StepFinishPart` (`finish-step`), and finally a `FinishPart` (`finish`).

A text block is a `TextStartPart` (`text-start`), one or more `TextDeltaPart` values (`text-delta`), and a `TextEndPart` (`text-end`, with the block's `Citations`). A reasoning block is a `ReasoningStartPart`, `ReasoningDeltaPart` values, and a `ReasoningEndPart`. Every one of them has the block's `Id` and `ProviderMetadata`. The id is the provider's block id, made unique across the steps of the stream. A provider that sends a delta without a start, or never ends a block, still gets the start and end parts, so `ToUIMessageChunk` maps each part to one chunk.

A tool call that succeeds is a `ToolResultPart` (`tool-result`). One that fails or is denied is a `ToolErrorPart` (`tool-error`), and its `Result.OutputJson` holds the error. This includes provider-executed tools, which are streamed in the order the provider sent them. Their `ExecutedTool` has `ProviderExecuted`, `ProviderMetadata`, and the call's `InputJson`. The results of client tools stream when the model call ends, as each execution completes (completion order, not call order), and a tool that streams output also emits its preliminary results. A step lists its tool results in the same completion order.

`StartStepPart.Request` is a `LanguageModelRequestMetadata` with `Body` (the HTTP body sent to the provider) and `Messages` (the messages sent for the step). Both are `null` unless you set `StreamTextOptions.Include` to a `StreamTextInclude` with `RequestBody` or `RequestMessages`, because they can be large when the prompt has images or files.

`StepFinishPart.Response` is a `LanguageModelResponseMetadata` with the response `Id`, `ModelId`, `Timestamp`, and HTTP `Headers` (`null` for a provider that does not use HTTP). A provider's `ResponseMetadataStreamPart` sets them. Without one, `Id` is generated, `Timestamp` is the step start, and `ModelId` is the requested model. `RawFinishReason` is the provider's own finish reason, and `ProviderMetadata` is the step's provider metadata.

`FinishReason.Interrupted` (`"interrupted"` on the wire) means the provider stream ended without a completion signal, so the output may be incomplete. The partial text and tool calls are kept. A required or named `ToolChoice` is not enforced for the step, no client tool runs, and the tool loop stops. Providers report it when a stream closes cleanly before their finish event. A stream that produces no output and no finish part gets no interrupted finish from the Anthropic provider (see #769 for the core no-output rejection).

A step carries the same data afterwards. `StepResult.Request` is the step's `LanguageModelRequestMetadata`, `StepResult.Response` is a `StepResponse` (the response metadata plus `Messages`, the response messages the step added, and `Body`, which streams leave unset), and `StepResult.RawFinishReason` is the provider's own finish reason. `StreamTextResult.Request` and `StreamTextResult.Response` are tasks that resolve with the last step's `Request` and `Response`, and `GenerateTextResult.Request` and `GenerateTextResult.Response` are the last step's too. `StreamTextResult.ResponseMessages` resolves with the response messages of all steps (the assistant message of each step and its tool message), like `GenerateTextResult.ResponseMessages`. Each reasoning block of a step keeps its own part in the assistant message, with the latest provider metadata of its start, delta, and end parts as provider options. This is how an OpenAI Responses reasoning `itemId` and encrypted content come back on the next step.

Set `StreamTextInclude.RawChunks` to receive the provider's raw stream chunks as `RawPart` (`raw`) values with a `RawValue`. It also asks the model to include them. It is `false` by default.

`PingPart` (`ping`) is a transient provider heartbeat. It carries no generated content, does not reset the model-output timeouts (`ChunkMs`, `FirstChunkMs`), is not stored in `StepResult.Content` or the text, and is left out of UI message streams. A step that only receives pings still ends with `NoOutputGeneratedException`. A provider emits it as a `PingStreamPart` and may require an opt-in, such as `providerOptions.anthropic.includePings`. Handle `PingPart` in every `switch` over `TextStreamPart` before you enable it. A `ping` does not add keepalives to UI responses: send SSE comments at your transport boundary. A custom `ILanguageModel` may emit `PingStreamPart` as a heartbeat with no generated content or usage.

A tool that has no `Execute` is for the caller to run. Its call is streamed as a `ToolCallPart`, and the step has no result for it.

`StepResult.Performance` is a `StepPerformance` for the step (`StepFinishPart.Performance` is the same value). `GenerateTextAsync` fills it too, without the streaming-only metrics (`TimeToFirstOutputMs`, `TimeBetweenOutputChunksMs`, `OutputTokensPerSecond`, `InputTokensPerSecond`). It has: `StepTimeMs`, `ResponseTimeMs`, `ToolExecutionMs` (by tool call id), `TimeToFirstOutputMs`, `TimeBetweenOutputChunksMs` (`Min`, `Max`, `Avg`, `Median`, `P10`, `P90`), and the token rates `EffectiveOutputTokensPerSecond`, `EffectiveTotalTokensPerSecond`, `OutputTokensPerSecond` and `InputTokensPerSecond`. The first-output metrics are `null` when the step produced no output chunk.


`UIMessageStream` writes the citations of a step on its `text-end` chunk (`citations`) and still writes retrieved URL sources as `source-url`. It does not write a `source-document` chunk, so document sources stay on `StepResult.Content` and the model stream only. Document sources are not in `Sources`, which holds URL sources.

Providers that do not send citations leave `Citations` null. A null value does not mean `Sources` is a complete retrieval set.

## Steering

Steering is experimental. It adds a user message to a running loop. Use it when a user sends a correction or a follow-up while the agent is still working. The message is added at the next step boundary, after the current step and its tool calls are done, so running tools are never interrupted.

Create a steering object with `Steering.ExperimentalCreateSteering()`, pass it as `ExperimentalSteering`, and call `SteerAsync` while the loop is running:

```csharp
var steering = Steering.ExperimentalCreateSteering();

var result = client.StreamTextAsync(new StreamTextOptions
{
    Model = model,
    Tools = [weather],
    StopWhen = StopWhen.IsStepCount(10),
    Prompt = "What is the weather in Paris?",
    ExperimentalSteering = steering,
});

// e.g. when the user sends another message while the agent is running:
var outcome = await steering.SteerAsync("I meant Paris, Texas.");
```

`GenerateTextAsync` takes the same option. `Agent.GenerateAsync` and `Agent.Stream` take it as the `experimentalSteering` parameter. It is not passed to `AgentOptions.PrepareCall`.

`UIMessageStreamConverter.CreateAgentUIStreamAsync(new CreateAgentUIStreamOptions { Agent = agent, UIMessages = messages, ExperimentalSteering = steering })` (in `Vercel.AI.AspNetCore`) runs an agent on UI messages and returns the UI message chunks. It validates the messages, converts them, streams the agent, and accepts `ExperimentalSteering`. Steering is only supported by `Agent`. Another `IAgent` makes the call throw `InvalidArgumentException`.

The options also take `UIMessagesJson` (a JSON array that the call validates, like the `unknown[]` of `uiMessages`), `ConvertDataPart` (converts custom data parts to text or file parts, and data parts are ignored when it is omitted or returns `null`), `ExperimentalSandbox` (passed through to tool execution), `OnStepEnd` (called when each generation step ends, together with `AgentOptions.OnStepEnd`), `OnUIMessageStepEnd` (called with the accumulated UI message after each step), and `UIMessageStreamOptions`. `IAgent.Stream` takes `experimentalSandbox` as well.

`UIMessageStreamConverter.CreateAgentUIStreamResponse` takes the same options plus `Headers`, `Status`, `StatusText`, `KeepAliveMs`, and `ConsumeSseStream`, and returns a `UIMessageStreamResponse` whose body is the stream as Server-Sent Events. `UIMessageStreamConverter.PipeAgentUIStreamToResponse` takes the same options plus a `Response` (a `ServerResponse`) and writes the stream to it. `UIMessageStreamResponses.PipeUIMessageStreamToResponse` does the same for any UI message chunk stream.

- Messages are added in the order they are sent. Each message is its own user message.
- If the model finishes its answer while a message is waiting, the loop continues for another step.
- Stop conditions still apply. `GenerateTextAsync` and `StreamTextAsync` stop after one step by default, so set `StopWhen` to steer them.
- Messages that are sent before the loop starts are added to the first step.
- `PrepareStepContext.Messages` already contains the steering messages of the step. `PrepareStepContext.ExperimentalSteeringMessages` lists them.
- A steering object can only be used for one call. Create a new one with `Steering.ExperimentalCreateSteering()` for each call. A second call with the same object throws `InvalidArgumentError`.
- A steering object that does not come from `Steering.ExperimentalCreateSteering()` throws `InvalidArgumentError`.

If a `PrepareStep` returns a `Messages` override that is rebuilt from other fields, such as `InitialMessages` and `ResponseMessages`, the steering messages are dropped and `SteerAsync` reports them as not delivered with the reason `RemovedByPrepareStep`. Build overrides from `Messages`, or add the steering messages of every step back in order:

```csharp
PrepareStep = context => new PrepareStepUpdate
{
    Messages =
    [
        .. context.InitialMessages,
        .. context.Steps.SelectMany(step => (step.ExperimentalSteeringMessages ?? []).Cast<ModelMessage>().Concat(step.Response.Messages)),
        // steering messages for the current step:
        .. context.ExperimentalSteeringMessages ?? [],
    ],
},
```

### Interrupting the current model call

A steering message waits for the current step by default. Pass `new SteerOptions { Interrupt = true }` to cut the current model call short when the model has not produced any output yet, e.g. for a correction like "stop, use the other file":

```csharp
await steering.SteerAsync("Stop, use the other file.", new SteerOptions { Interrupt = true });
```

- When the model call has not produced any text, reasoning, tool input, or tool call yet, it is cancelled and made again in the same step with the message. The step keeps its step number and does not count toward stop conditions.
- `GenerateTextAsync` receives the output of a model call all at once, so the call can be cancelled until it returns. A response that is almost complete is discarded.
- When output has already started, or tools are running, the message is added at the next step boundary like any other steering message. Running tools are never interrupted. Stop conditions apply to that next step: with the default of one step, the message is not delivered and `SteerAsync` reports the reason `StopCondition`.
- `PrepareStep` runs again for the new model call and receives the message in `PrepareStepContext.ExperimentalSteeringMessages`. It gets the same instructions and messages as its first call for the step.
- `OnLanguageModelCallStart` is called again for the new model call. `OnLanguageModelCallEnd` is not called for the cancelled one.
- `StreamTextAsync` closes the text and reasoning parts of the cancelled call and streams the `SteeringMessagePart` before the new output. A part that was closed before it got any content stays in the UI message as an empty text or reasoning part. It streams one `StartStepPart` and one `FinishStepPart` for the step.
- A caller abort or a timeout wins over an interrupt: the call ends and the message is reported as not delivered with the reason `Aborted`. The step timeout keeps running when the step is redone.
- A cancelled model call can still be billed by the provider. It is not included in the usage of the result.
- `QueueAsync` takes `QueueOptions`, which has no `Interrupt`, because a queued message waits for the task to finish.

> Stop conditions and usage do not limit interrupts. Each interrupt makes another model call in the same step, so a stop condition like `StopWhen.IsStepCount` does not count it, and the usage of the result leaves out the cancelled calls. If your users can send interrupting messages, limit how many you accept for one call and set a `Timeout`.

### Queueing messages

`SteerAsync` changes what the agent is doing right now. Use `QueueAsync` for a follow-up that should wait until the agent has finished its current task. A queued message is added once the model finishes its current task, i.e. when it gives a final answer without tool calls. The loop then continues with the message, so the agent works through the queue one task at a time:

- Queued messages are added one per task, in the order they are queued.
- Waiting steering messages are added first. A queued message waits for the next task that finishes.
- Messages that are queued before the loop starts wait until the model has answered the prompt.
- A response that is cut off (`FinishReason.Length` or `FinishReason.ContentFilter`) does not finish the task, so the loop ends and the queued message is reported as not delivered.
- Stop conditions still apply, and a queued message that the loop does not reach is reported as not delivered.

### Handling the outcome

`SteerAsync` and `QueueAsync` return a `Task<SteeringOutcome>` that never faults. It is a `DeliveredSteeringOutcome` (`Id`, `Message`, `StepNumber`) or a `NotDeliveredSteeringOutcome` (`Id`, `Message`, `Reason`). `SteeringOutcome.Type` is `delivered` or `not-delivered`. A message is delivered when it is part of the messages that are sent to the model, after `PrepareStep`. `Reason` is a `SteeringNotDeliveredReason`:

- `Finished`: the loop ended before the message could be added.
- `RemovedByPrepareStep`: `PrepareStep` returned messages without it.
- `StopCondition`: a stop condition ended the loop.
- `WaitingForToolResults`: the loop ended to wait for tool results, e.g. a tool approval.
- `Aborted`: the loop was aborted.
- `Error`: the loop failed.

Pass `new SteerOptions { Id = id }` (or `new QueueOptions { Id = id }`) to make retries safe. Sending a message again with the same `Id` returns the outcome of the first message. A message that is sent again does not interrupt again.

### Saving and showing steering messages

Steering messages are not part of `ResponseMessages`. Each `StepResult` lists the messages that were added to it in `ExperimentalSteeringMessages`, so you can rebuild the conversation in order. `StreamText` emits a `SteeringMessagePart` before the `StartStepPart` of the step that received the message. `ToUIMessageChunk` converts it to a `steering-message` chunk, `ReadUIMessageStream` shows it as a `SteeringMessageUIPart`, and `ConvertToModelMessages` turns it back into a user message between the steps of the assistant message.

## Smooth streaming

`SmoothStream.Create(new SmoothStreamOptions { ... })` maps to `smoothStream`. Pass it to `StreamTextOptions.ExperimentalTransform` (maps to `experimental_transform`) to smooth the output of `StreamTextAsync`: `ExperimentalTransform = [SmoothStream.Create()]`. The returned `StreamTextTransform` takes `StreamTextTransformOptions` (`Tools` and `StopStream`) and returns a transform from one full stream to another: `SmoothStream.Create()(new StreamTextTransformOptions(tools))(stream, cancellationToken)`. It buffers `TextDeltaPart` and `ReasoningDeltaPart` text and emits it word by word (`SmoothStreamChunking.Word`, the default) or line by line (`SmoothStreamChunking.Line`), waiting `DelayInMs` (default 10, `null` skips the wait) after each chunk. `ChunkingRegex` (a pattern that must not match the empty string) and `ChunkDetector` (a function that returns a non-empty prefix of the buffer, or `null`) chunk the text your way. Any other part flushes the buffered text first and then passes through. A `PingPart` passes through at once and does not flush a partial word. Deltas that carry provider metadata are flushed at the metadata boundary, so each output part keeps the metadata of its source delta. `Segmenter` takes an `ISegmenter` (maps to `Intl.Segmenter`; its `Segment` method returns the segments of a string and only the first is read) for locale-aware word segmentation, and takes precedence over the other chunking options. .NET has no built-in word segmenter, so you supply one. Upstream skips the delay while a browser document is hidden (`isDocumentHidden`). .NET has no document, so the port never skips it for that reason.

## Stream result

`ILanguageModel.DoStreamAsync` returns `Task<LanguageModelStreamResult>`, as upstream `doStream` returns `{ stream, request, response }`. `Stream` holds the `LanguageModelStreamPart` values (the result is also directly enumerable), `Request.Body` holds the JSON request body, and `Response.Headers` holds the HTTP response headers. Both `Request` and `Response` are null when a model does not report them. Amazon Bedrock, Anthropic, Cohere, Gateway, Google, Hugging Face Responses, OpenAI (chat, completion, and responses), OpenResponses, Perplexity Agent, xAI Responses, and the OpenAI-compatible chat and completion models fill them in. These models send the request inside `DoStreamAsync`, so an HTTP error is thrown by that call and not while you read the stream. Dispose the result when you stop without reading the stream to the end. A language-model middleware's `WrapStreamAsync` takes and returns the same result type, so a middleware that transforms the stream passes `Request` and `Response` along.

A provider opens the HTTP response when `DoStreamAsync` returns, so HTTP errors surface from that call. `LanguageModelStreamResult` implements `IAsyncDisposable` and `IDisposable`. Dispose it to release the response when you drop a result without enumerating it. Disposing is safe to repeat, and it does nothing after the stream ends. `StreamTextAsync` and the agent dispose the result for you.

## Transforming the stream

`StreamTextOptions.ExperimentalTransform` takes a list of `StreamTextTransform` functions (maps to `experimental_transform`). Each receives `StreamTextTransformOptions` with the `Tools` of the call (empty when none) and `StopStream`, and returns a function from the stream of `TextStreamPart` to a new stream. The transforms run in order, and they must keep the stream structure intact. After a transform calls `StopStream`, no further part from the model reaches the transforms, so the stream ends once they flush.

```csharp
StreamTextTransform stopWord = options => (stream, ct) => StopAtWord(stream, options.StopStream, ct);
var result = client.StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "...", ExperimentalTransform = [stopWord] });
```

The transforms run before the event processor, as upstream's `experimental_transform` does. The event processor records the steps, settles the result tasks, and calls the callbacks from the transformed stream. So `Text`, `Steps`, `Response`, `ResponseMessages`, `Usage` (the `FinishPart` usage), `FinishReason` (the `FinishPart` finish reason), `ProviderMetadata`, `OnChunk`, `OnStepEnd`, and `OnEnd` all see what the transforms produced, and the next step of a tool loop is prompted with the transformed response messages. A transform that changes a `ToolCallPart`, a `ToolResultPart`, or the `StepFinishPart` of a step (build a new `StepResult` for it) changes the recorded step. The step is recorded from the parts, so a transform must pass the `StepFinishPart` on. After a transform calls `StopStream`, the results settle with what the transforms let through, like upstream's `terminate`.

## Chunk callback

`StreamTextOptions.OnChunk` (maps to `onChunk`) is called for each part of the stream after the transforms. It receives a `StreamTextChunkEvent` with the `Chunk`, and the stream processing waits until it returns.

## Retrying after the stream started

`MaxRetries` retries a model call that fails to start. `StreamTextOptions.StreamRetries` (maps to `streamRetries`) retries the current model step after a provider `ErrorPart` that arrives from the response stream. Leave it `null` to disable all stream retry behavior. It must be 0 or more. A retry reruns only the failed step with the same prompt. Earlier steps and their tool results are kept. The tool parts of an attempt wait until the attempt reaches its model-call end, so a failed attempt starts no tool. Text and reasoning that the failed attempt already streamed stays in `Stream()` and `TextStream()`, its open text and reasoning parts are ended, and what it produced is left out of the recovered step, the structured output, the response messages, and the next steps. The request and response metadata of the step come from the recovered attempt.

`StreamTextOptions.OnErrorRetry` (the `onError` that returns `{ retry: true }`; C# has no union of callbacks) takes the place of `OnError` and returns a `StreamTextOnErrorResult`. With `StreamRetries` set, `Retry = true` retries once more, even with `StreamRetries = 0`. At most one retry per step is directed by the callback, after the automatic retries are used up. A retried error is not sent as an `ErrorPart`, and `OnError` or `OnErrorRetry` is called once for each error part. Without `StreamRetries`, `OnErrorRetry` only observes errors.

```csharp
var result = client.StreamTextAsync(new StreamTextOptions
{
    Model = model,
    Prompt = "...",
    StreamRetries = 0,
    OnErrorRetry = (error, ct) => Task.FromResult<StreamTextOnErrorResult?>(
        IsTransient(error) ? new StreamTextOnErrorResult { Retry = true } : null),
});
```
