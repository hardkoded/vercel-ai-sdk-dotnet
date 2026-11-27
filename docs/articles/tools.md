# Tools

A tool has a name, a JSON Schema, and an `Execute` delegate. The model receives the schema. The SDK runs `Execute` when the model calls the tool, then feeds the JSON result back as a tool message.

<!-- snippet: tools -->
<a id='snippet-tools'></a>
```cs
var client = new AiClient(GatewayProvider.Create());

var weather = Tool.Function(
    "weather",
    "Returns a short forecast.",
    """{ "type": "object", "properties": { "city": { "type": "string" } }, "required": ["city"] }""",
    (arguments, cancellationToken) =>
    {
        var city = arguments.GetProperty("city").GetString();
        return Task.FromResult(JsonSerializer.Serialize(new { city, forecast = "sunny" }));
    }
);

var result = await client.GenerateTextAsync(
    new GenerateTextOptions
    {
        ModelId = "openai/gpt-6-astra",
        Prompt = "Weather in Lisbon?",
        Tools = new[] { weather },
        StopWhen = StopWhen.IsStepCount(4),
    }
);
```
<sup><a href='/examples/Tools/Program.cs#L15-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-tools' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The default stop condition is one step. That step’s tool calls still run, and the loop stops before a second model call. Raise `IsStepCount`, or use `HasToolCall` / `IsLoopFinished`, when the model should see the tool result.

`Agent` stops after 10 steps when `AgentOptions.StopWhen` is null. When that default limit is what stopped the loop, the agent logs one warning through `LogWarnings` (type `other`, with the provider and model of the last step). Set `StopWhen` explicitly, with `IsStepCount` or a custom condition, to raise the limit and silence it. `GenerateTextAsync` and `StreamTextAsync` do not log. `LogWarnings.Logger = false` suppresses the warning. `StepResult.Provider` and `StepResult.ModelId` name the model that produced each step.

`GenerateTextOptions.Instructions`, `StreamTextOptions.Instructions`, and `AgentOptions.Instructions` take an `Instructions` value (`Vercel.AI.Prompt`): a string, one `SystemModelMessage`, or an array of `SystemModelMessage`. Each converts implicitly, so `Instructions = "Be brief."` still works. Use a system message to attach provider options to the instructions. An array becomes several leading system messages, in order. `Instructions` wins over `System`. Code that reads the property now gets an `Instructions`: use `Text` for a string and `Messages` for system messages. `ToolCallRepairContext.Instructions` and `System` are `Instructions` too, so a repair function reads `.Text` or `.Messages` there as well. The string conversion rejects null: assigning a null `string?` variable to `Instructions` throws `ArgumentNullException`. Leave `Instructions` unset when there are none.

System messages in `Messages` are rejected with an `InvalidPromptException` ("System messages are not allowed in the prompt or messages fields. Use the instructions option instead."). Set `AllowSystemInMessages = true` on `GenerateTextOptions` or `StreamTextOptions` to allow them, for example to replay a history that holds a system message with provider options such as `openai.textFormatUpdate`. Maps to `allowSystemInMessages`. `AgentOptions.AllowSystemInMessages` does the same for `Agent.GenerateAsync(messages)` and `Agent.Stream(messages)`, which take a `IReadOnlyList<ModelMessage>` instead of a string prompt. `PrepareCall` can replace it by setting `AllowSystemInMessages` on the options it returns. Maps to `allowSystemInMessages` of `ToolLoopAgentSettings`.

`StreamTextResult` exposes `Output` (the parsed structured output) and `ProviderMetadata` (of the last step) as tasks that complete when the stream finishes, and `ConsumeStreamAsync()` to read the stream without processing the parts. They map to `output`, `providerMetadata` and `consumeStream`.

Set `AgentOptions.PrepareCall` to change the options of each `Agent.GenerateAsync` and `Agent.Stream` call. It receives the options the agent would use, including the default `StopWhen`, and returns replacement options (or null to keep them). Returning them unchanged keeps the default warning; setting `StopWhen` replaces the default limit and silences it.

`AgentOptions` also takes the call settings of `GenerateTextOptions`: `TopP`, `TopK`, `PresencePenalty`, `FrequencyPenalty`, `StopSequences`, `Seed` and `ProviderOptions`, plus `ExperimentalDownload` (maps to `experimental_download`) and `Include` (maps to `include`). `Include` is an `AgentInclude`, which adds `RawChunks` to `GenerateTextInclude`: `Agent.GenerateAsync` uses the request and response settings, and `Agent.Stream` also forwards `RawChunks` to the model as `IncludeRawChunks`. `PrepareCall` can replace them.

`Agent<TCallOptions>` takes per-call options, like `ToolLoopAgent<CALL_OPTIONS>`. Build it with `AgentOptions<TCallOptions>`, which adds `CallOptionsSchema` (a JSON schema, maps to `callOptionsSchema`) and a `PrepareCall` of type `Func<GenerateTextOptions, TCallOptions, GenerateTextOptions?>` that receives the validated options. `GenerateAsync` and `Stream` take the options as a required second argument, so a call without options does not compile. The options are validated against `CallOptionsSchema` before `PrepareCall` and before the model runs. A mismatch throws a `TypeValidationException` ("Type validation failed for options"). `Agent<TCallOptions>` is not an `IAgent`, so `CreateAgentUIStreamAsync` does not accept it. Use the non-generic `Agent` when you need no call options.

A call to a tool that has no `Execute` ends the loop at that step. The stop condition is not evaluated. A call that waits for approval (see below) also ends the loop, and no warning is logged.

## Tool approval

Breaking change: `GenerateTextOptions.ApproveTool` and the `ToolApprovalRequest` class are removed. Use `ToolApproval`, which maps to upstream `toolApproval`. `AgentOptions` takes the same `ToolApproval`, `ToolsContext`, `RuntimeContext`, `ExperimentalToolApprovalSecret`, and `ExperimentalToolApprovalSecretBytes`, so `Agent.GenerateAsync` and `Agent.Stream` stop at a `user-approval` tool call like `GenerateTextAsync`. `PrepareCall` can replace them.

A tool needs approval when its `NeedsApproval` is true or its `NeedsApprovalFunction` returns true. The function receives the parsed input and a `ToolNeedsApprovalOptions` (`ToolCallId`, `Messages`, `Context`). `GenerateTextOptions.ToolApproval` overrides the tool's own setting. It is a `ToolApprovalConfiguration`, which takes either a generic `GenericToolApprovalFunction` or a per-tool map:

```csharp
ToolApproval = new ToolApprovalConfiguration
{
    { "deleteFile", "user-approval" },
    { "readFile", "approved" },
    { "runQuery", (input, options) => Task.FromResult<ToolApprovalStatus?>(
        input.GetProperty("write").GetBoolean() ? "user-approval" : "approved") },
},
```

A `ToolApprovalStatus` is `not-applicable`, `approved`, `denied` or `user-approval`, each with an optional `Reason`. A null result means `not-applicable`, so the tool's own setting decides. Precedence: the generic function, then the per-tool entry, then `NeedsApprovalFunction`, then `NeedsApproval`.

`GenerateTextOptions.ToolsContext` holds one JSON context per tool name and `RuntimeContext` holds your own context. A per-tool approval function receives them as `ToolContext` and `RuntimeContext` on its `SingleToolApprovalOptions`. A generic function receives `ToolsContext` and `RuntimeContext` on its `GenericToolApprovalOptions`. A tool's `ContextSchema` validates and normalizes its context before a per-tool or tool-defined function sees it. It returns the validated context, with defaults applied, and throws when the context is invalid. A rejected context throws `TypeValidationException` (`Context.Field` is `tool context`) before the approval function runs. The same validated context reaches the tool as `ToolExecutionOptions.Context` when it runs, and the tool execution callbacks carry it as `ToolContext`.

### Tool input lifecycle hooks

A tool can observe its input as the model generates it:

- `OnInputStart` runs when the model starts generating the input of the call.
- `OnInputDelta` runs for each fragment of the input text, as `InputTextDelta`.
- `OnInputAvailable` runs when the complete input is available, as `Input`, even when the tool has no executor. A call that failed validation (`Invalid`) does not reach it. It runs for the tool that the completed call names, which can differ from the tool that started the input when `RepairToolCall` renamed the call.

Each hook receives the `ToolCallId`, the `Messages` of the step, the `AbortSignal` and the `Context` of the tool, validated with `ContextSchema`. The context is validated once per tool call and shared by its hooks. `StreamTextAsync` calls all three. After a stream retry starts a new attempt, the tool calls of the failed attempt are forgotten. `GenerateTextAsync` calls `OnInputStart` and then `OnInputAvailable` for every valid call to a known tool of a step, with the validated tool context and before the approval is resolved. It does not call `OnInputDelta`, because a non-streaming response has no input deltas. A call that violates `ToolChoice` throws before any hook runs.

```csharp
var tool = Tool.Function("getWeather", "Get the weather", schema, execute);
tool.OnInputStart = options => { Console.WriteLine("Tool call starting"); return Task.CompletedTask; };
tool.OnInputDelta = options => { Console.WriteLine(options.InputTextDelta); return Task.CompletedTask; };
tool.OnInputAvailable = options => { Console.WriteLine(options.Input); return Task.CompletedTask; };
```

`Tool.DescriptionFactory` resolves the description per call, instead of the fixed `Description`. It receives a `ToolDescriptionContext` with the tool's entry of `ToolsContext` as `Context` and the `Description` of `ExperimentalSandbox` as `SandboxDescription`. This maps to a function-valued `description` upstream.

A tool's `InputTransform` is the transform of an input schema, such as a Zod `.transform`. It runs after the input matches `InputSchema` and returns the schema output, which the tool, the callbacks, and the approval check see. It throws when the input is invalid, which becomes an `InvalidToolInputError`. When an approved call is re-validated, the transform runs on the schema input, and the result must equal the approved input. `ValidateUIMessagesAsync` applies the transform to the validated input of a tool part, so a persisted UI message that keeps the schema input round-trips.

With `user-approval`, the step content holds a `GeneratedToolApprovalRequest` and the tool call does not run. The loop stops. To continue, send the result messages back with a `ToolApprovalResponseModelMessage` (`approvalId`, `approved`, optional `Reason`). `GenerateTextAsync` collects the responses, re-validates each approved call (signature, input schema, and the approval policy again), runs the approved calls, and gives denied calls an execution-denied result. An approved call with an invalid input becomes an error result instead. With `approved` or `denied`, the step content holds the request (`IsAutomatic = true`) and a `GeneratedToolApprovalResponse`, and the loop goes on when every client call is settled.

`StreamTextAsync` settles the approval responses at the end of the history the same way, before its first step, right after the `start` part. It emits a `ToolOutputDeniedPart` for each denied call (also a provider-executed one, and one that already has an execution-denied result), a `ToolErrorPart` for an approved call whose input is no longer valid, and the `ToolResultPart` parts of the approved calls, preliminary results first. `StreamTextResult.ResponseMessages` starts with the tool message of these results. Its `ToolExecutionOptions.AbortSignal` is the cancellation token of the stream.

Set `ExperimentalToolApprovalSecret` (or `ExperimentalToolApprovalSecretBytes`) to sign approval requests with an HMAC, except the requests of tool calls that the approval policy denied automatically. A response whose signature is missing or wrong throws `InvalidToolApprovalSignatureError`. Because an automatic denial is not signed, a client cannot send a denied tool call back as approved: that response throws `InvalidToolApprovalSignatureError` before any tool runs. Conversations that contain an automatic denial continue as before. Without a secret, signatures are ignored.

A prompt where a local tool call has no tool result before the next user or system message (or at the end) is rejected with `MissingToolResultsError`, whose `ToolCallIds` lists the calls. Provider-executed calls are exempt. When you continue a chat after an approval, make sure the approval response reaches the model call or drop the unresolved call from the history. On continuation, the approval policy sees the current tool's `Title`, `Metadata` and dynamic flag, not the values stored in the history.

## Tool choice none

When `ToolChoice` is `ToolChoice.None`, the SDK prevents local tool execution even if the model returns a tool call. Such a response throws `ToolChoiceViolationException` from `GenerateTextAsync`. With `StreamTextAsync`, the exception is an `ErrorPart` in the stream, reported to `OnError`, and `FinishReason` resolves to `FinishReason.Error` with the usage of the call. The tool definitions are still sent to the model. With `StreamTextAsync`, tool parts are withheld from the stream, so client-side tools are not invoked either. Tool call repair (`RepairToolCall`) and input refinement (`RefineToolInput`) are skipped for rejected tool calls. A `PrepareStep` that returns another `ToolChoice` replaces `None` for that step. The Hugging Face Responses provider sends `tool_choice: "none"`.

## Tool call execution concurrency

By default, when a model generates several tool calls in one step, `GenerateTextAsync`, `StreamTextAsync`, and `Agent` start all of them at once. Set `ToolCallConcurrency` to a positive integer to limit how many tool calls run at the same time. The property is on `GenerateTextOptions`, `StreamTextOptions` (which inherits it), and `AgentOptions`. `Agent` copies it onto the options it builds, so `PrepareCall` can replace it.

Use `ToolCallConcurrency = 1` when later calls need to see the side effects of earlier calls. The SDK then runs the calls one at a time, in the order the model generated them. Results always stay in model order, whatever the limit.

```csharp
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Prompt = "Update the file, then read it.",
    Tools = tools,
    ToolCallConcurrency = 1,
});
```

A value below 1 throws `InvalidArgumentError` (`Parameter` is `toolCallConcurrency`) before the model is called. `StreamTextAsync` throws before it returns.

The option controls the tool calls that the SDK runs itself. It does not limit how many tool calls the model generates, and it does not schedule provider-executed tools. Provider options such as `parallelToolCalls` control model generation instead. When the call is canceled, tool calls that are still queued do not start.

## Provider-executed tool approval

A provider-executed tool, such as an MCP tool, can ask for approval before the provider runs it. The model returns a `GeneratedToolApprovalRequest` (`tool-approval-request`) next to the tool call. The step keeps it in `Content` with its `ToolCall` filled in, and `StreamText` emits a `ToolApprovalRequestPart`. The step ends there, because there is no client tool call to run, and the finish reason stays `tool-calls`.

`GenerateTextResult.ResponseMessages` holds the messages the call added. `StreamTextResult.ResponseMessages` is the same list as a task that completes when the stream finishes. The assistant message keeps an `AssistantToolApprovalRequestPart` after the tool call. To continue, send the history back with a `ToolApprovalResponseModelMessage` that has `ProviderExecuted = true`:

```csharp
new ToolApprovalResponseModelMessage("mcp-approval-1", approved: false, reason: "User denied the request", providerExecuted: true)
```

The model never sees approval requests. A response without `ProviderExecuted` is dropped from the prompt. A denied response reaches the model with its `DeniedToolResult`, an `execution-denied` tool result whose output provider options are `{ "openai": { "approvalId": ... } }`.

## Pruning messages

`Ai.PruneMessages` removes reasoning, tool calls, tool results, and approvals from a message list, for example to shorten a long history before the next call. It returns a new list and leaves the input unchanged.

```csharp
var pruned = Ai.PruneMessages(new PruneMessagesOptions
{
    Messages = messages,
    Reasoning = PruneReasoning.All,
    ToolCalls = new[] { PruneToolCallsRule.BeforeLastMessages(2, new[] { "weather" }) },
});
```

- `Reasoning`: `PruneReasoning.None` (default), `All`, or `BeforeLastMessage`, which keeps the reasoning of the last message. Reasoning means `AssistantReasoningPart` and `AssistantReasoningFilePart`. Regular `AssistantFilePart` parts stay.
- `ToolCalls`: a list of rules. `PruneToolCallsRule.All()`, `BeforeLastMessage()`, and `BeforeLastMessages(n)` take an optional list of tool names. Without names a rule prunes every tool. Calls, results, and approvals in the kept trailing messages stay, and so does the originating tool call of a kept approval response. An approval response whose request is unknown is pruned when the rule names tools.
  Reasoning (`AssistantReasoningPart` and `AssistantReasoningFilePart`) that only precedes pruned content is pruned too. A run of consecutive reasoning parts is one group: it goes when everything after it, up to the next reasoning part, was pruned, and stays when any of that content stays or when nothing follows it.
- `EmptyMessages`: `PruneEmptyMessages.Remove` (default) drops messages that are empty after pruning, `Keep` keeps them.

Consecutive tool messages (results and approval responses) count as one message, as upstream's tool message holds several parts.

## Converting UI messages

`Ai.ConvertToModelMessages` converts the `UIMessage` list of a chat UI into model messages for `Ai.GenerateText` and friends. A `UIMessage` holds parts: `TextUIPart`, `ReasoningUIPart`, `FileUIPart`, `ToolUIPart`, `DataUIPart`, `StepStartUIPart`, and others. A `ToolUIPart` covers both `tool-{name}` and `dynamic-tool` parts.

```csharp
var messages = Ai.ConvertToModelMessages(
    uiMessages,
    new ConvertToModelMessagesOptions { IgnoreIncompleteToolCalls = true });
```

- `Tools`: the tools by name. A `dynamic-tool` part with `Dynamic = false` whose tool is missing gets the output `Tool output omitted because the tool is no longer available.`
- `IgnoreIncompleteToolCalls`: drops tool parts that have no final result.
- `ConvertDataPart`: turns a `DataUIPart` into a text or file part. Return `null` to ignore the part.

A step-start part ends a block. Each block becomes an assistant message, followed by the approval responses and tool results of its tools, each as its own tool message.

- Assistant `FileUIPart` parts become `AssistantFilePart` (a `ProviderReference`, when set, replaces the URL, as for user files) and `ReasoningFileUIPart` parts become `AssistantReasoningFilePart`.
- The approval request keeps its id, tool call id, and `IsAutomatic`. It also gets `Reason` from `ToolUIApproval.RequestReason`, `Signature` from `ToolUIApproval.Signature`, and `InputSchemaInput` from `ToolUIApproval.InputSchemaInput` when they are set. `ToolUIApproval.Descriptor` is kept on the UI part and is not copied.
- When a tool in `Tools` sets `Tool.ToModelOutput`, it converts the output of a finished call (it receives the output as JSON text). Failed calls and calls whose tool is missing do not use it.
- A failed call (`output-error`) with no `Input` falls back to `RawInput`. A string `RawInput` is the tool-call input as it is, not JSON-quoted text. This fallback is deprecated: the conversion logs a `DeprecatedWarning` (`AISDK_DEP_UI_MESSAGE_RAW_INPUT`) when it sees a `RawInput` on such a part.

## Tool errors

A tool result carries an output type: `text`, `json`, `error-text`, `error-json`, or `execution-denied`. `ToolModelMessage.OutputType` holds it. When you do not set it, the type comes from `isError` and from whether the output is valid JSON.

For `execution-denied`, `ToolModelMessage.ExecutionDeniedReason` holds the reason. It is `null` when the output has no reason, which differs from an empty reason. A tool result built from a list of parts accepts text parts, file parts (including `FileContentPart.FromText`), and `CustomContentPart` for provider-specific content. The deprecated `FileUrlContentPart` (`file-url`) and `ImageUrlContentPart` (`image-url`) parts still work: they are downloaded and inlined like a `FileContentPart` with a URL, and a deprecation warning is logged. `FileUrlContentPart` infers the media type from the URL extension when you do not set one, and `ImageUrlContentPart` uses the `image` media type. The other deprecated parts convert to a `FileContentPart` too, each with a deprecation warning: `FileDataContentPart` (`file-data`) and `ImageDataContentPart` (`image-data`) carry base64 data, and `FileIdContentPart` (`file-id`), `FileReferenceContentPart` (`file-reference`), `ImageFileIdContentPart` (`image-file-id`) and `ImageFileReferenceContentPart` (`image-file-reference`) become a provider reference. The `file-*` parts use the `application` media type and the `image-*` reference parts use `image`. A `FileIdContentPart` or `ImageFileIdContentPart` built from a single string id is keyed by the provider of the model, so it only works in a model call. Pass a map from provider name to file id to avoid that. Use `FileContentPart` instead. A provider skips the parts it does not support and adds a warning.

Providers keep the error status:

- OpenAI Chat Completions and Responses send `error-text` and `error-json` as `{"error": <value>}`. `execution-denied` stays the reason, or `Tool call execution denied.` when there is none.
- OpenAI Responses honors `promptCacheBreakpoint` on a scalar tool result (`text`, `json`, `error-text`, `error-json`, `execution-denied`). Set it in the provider options of the `ToolModelMessage` (`ProviderMetadata`) or of its output (`OutputProviderOptions`); the output's wins. The result is then sent as an `input_text` array with `prompt_cache_breakpoint`.
- A `LanguageModelTool` with `OutputSchema` is sent to OpenAI Responses as `output_schema`. Its `text` and `execution-denied` results are then sent as JSON string literals, because OpenAI parses the result as JSON.
- Google sends `error-text`, `error-json`, and `execution-denied` on `functionResponse.response.error`. Successful results stay on `response.content`.

## Tool order

`ToolOrder` on `GenerateTextOptions`, `StreamTextOptions` and `AgentOptions` controls the order in which tools are sent to the provider. The list can be partial. Tools not listed are sent after the listed tools, sorted alphabetically. Without it, tools keep the order you passed them in. A stable order can improve provider-side caching. `PrepareStepUpdate.ToolOrder` overrides it for one step.

```csharp
await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Tools = [zebra, alpha, middle],
    Prompt = "...",
    ToolOrder = ["middle"], // sent as middle, alpha, zebra
});
```

## Tool input validation

Before the loop runs a tool, `ToolCallParser.ParseToolCallAsync` parses the call's input and validates it against the tool's `InputSchema`. Empty input counts as `{}`. A call that fails is not executed. It comes back with `Invalid` and `Dynamic` set and the failure in `Error`: a `NoSuchToolError` when the tool is unknown (`AvailableTools` is null when no tools were set), or an `InvalidToolInputError` when the input is not valid JSON or does not match the schema. `GenerateTextAsync`, `StreamTextAsync`, and `Agent` run this parse on every tool call of a model response, with the tools the model was offered (`ActiveTools` narrows them). The validator covers `type`, `enum`, `const`, `properties`, `required`, `additionalProperties`, `items`, `minItems`, `maxItems`, `minLength`, `maxLength`, `minimum`, `maximum`, `anyOf`, `oneOf` and `allOf`.

`ParseToolCallOptions` also takes:

- `RepairToolCall`: called once for a `NoSuchToolError` or `InvalidToolInputError`. It gets a `ToolCallRepairContext` (the call, the tools, `InputSchema(toolName)`, the error, `Messages`, `Instructions` and a `CancellationToken`) and returns a repaired `GeneratedToolCall`, or null to keep the error. If it throws, the result carries a `ToolCallRepairError`. Cancelling the token stops the wait and cancels `ParseToolCallAsync`.
- `RefineToolInput`: a map from tool name to a function that returns a refined input for a valid call.

On `GenerateTextOptions` and `StreamTextOptions`, `ExperimentalRepairToolCall` is the deprecated alias of `RepairToolCall` (`experimental_repairToolCall` upstream). `RepairToolCall` wins when both are set.

A `Tool` can carry a `Title` and `Metadata`. A parsed call copies both as `Title` and `ToolMetadata`; `ToolMetadata` stays apart from the model's `ProviderMetadata`. `Tool.DynamicTool(...)` declares a dynamic tool, and its parsed calls have `Dynamic` set. A provider-executed `GeneratedToolCall` with `Dynamic` set parses even when no tool matches.

The generation loop passes both options on from `GenerateTextOptions` and `StreamTextOptions` (`RepairToolCall` and `RefineToolInput`), and from `AgentOptions`. The repair function gets the messages of the step and the instructions. A repaired or refined call replaces the model's call everywhere: in `StepResult.ToolCalls` and `Content`, in the `OnLanguageModelCallEnd` event, in the stream parts, in the executed tool's `InputJson`, and in the assistant message of `ResponseMessages`.

A call that stays invalid is kept in the step as a `GeneratedToolCall` with `Invalid` set. It is never executed and never asks for approval. The loop answers it with an error tool result (`ExecutedTool.IsError`, also a `GeneratedToolResult` in `Content`, and a `ToolErrorPart` in the stream). The model reads the error as `error-text` in the form `Name: message`, for example `AI_NoSuchToolError: Model tried to call unavailable tool 'x'. No tools are available.`, without a stack trace. A failing repair function gives a `ToolCallRepairError` whose message names the failure the same way. The loop then goes on to the next step, because every call has an answer.

In the UI message stream an invalid call is a `tool-input-error` chunk, followed by a `tool-output-error` chunk. Other tool calls carry `title`, `toolMetadata`, and `dynamic` when they have them, and so do `tool-input-start` (`title`, `toolMetadata`, `dynamic`) and the tool output chunks (`toolMetadata`, `dynamic`). An invalid call is marked `dynamic` only when its tool is unknown, because the UI writer has no tool list.

## Provider-defined tools

A provider-defined tool is run by the provider, such as a web search. Declare it with `Tool.ProviderDefined(id, name, args)`. The `id` is `<provider>.<tool>` and `args` is a JSON object. The tool has no `Execute`, and the SDK never runs it.

```csharp
using var args = JsonDocument.Parse("{}");
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "What happened today?",
    Tools = new[] { Tool.ProviderDefined("xai.web_search", "web_search", args.RootElement) },
});
```

The model returns the call with `GeneratedToolCall.ProviderExecuted` set, followed by a `GeneratedToolResult` that has `IsError` when the tool failed. Both stay in the step `Content`, in order. Successful results also appear in the step `ToolResults`. Calls the provider ran do not start another step. When a step also has a client tool call, the next step replays the provider's call and result in the assistant message (`AssistantToolCallPart` and `AssistantToolResultPart`, with their provider metadata).

Set `Tool.SupportsDeferredResults` on a provider tool whose result may arrive in a later step, such as code execution in programmatic tool calling. While a provider-executed call of such a tool has no result yet, the loop keeps going (subject to the stop condition) instead of ending, and it drops the call once its result arrives.

A provider tool declared with an input schema (the four-argument `Tool.ProviderDefined`) validates the input of its provider-executed calls, after it drops the properties the schema does not declare (it keeps them when `additionalProperties` is `true` or a schema). A call that does not match comes back in `Content` with `Invalid`, `Dynamic` and `Error` (an `InvalidToolInputError`) set. The provider already ran it, so the loop adds no client tool error and keeps the provider's own result.

In the UI message stream, provider-executed chunks are marked `providerExecuted`, and an error chunk keeps the provider's error text or JSON instead of the `onError` text. A provider-executed tool approval request (MCP) is not supported ([#542](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/542)).

## Gateway Tako search tool

`GatewayTakoSearch.Create(config)` declares the AI Gateway's Tako Search tool, a provider-defined tool (`gateway.tako_search`) that searches the web and Tako's curated data. `GatewayTakoSearchConfig` sets the search options up front. `VariableCost`, `MaxVariableCostUsd` and `ExtraBody` are developer-only: the model can't see or set them, and they are not in the tool input schema.

```csharp
var tool = GatewayTakoSearch.Create(new GatewayTakoSearchConfig
{
    MaxVariableCostUsd = 0.1,
});
```

- **Effort** _string?_ - `deep`, `fast` or `instant`. `fast` is the default balanced option.
- **Sources** _TakoSearchSourcesConfig?_ - Sources to search. Omit to search both curated data and the web. `Data` (`TakoDataSourceConfig`) has `Count` (1-20), `IncludeContents`, `Mode` (`inline` or `url`), `ContentFormat` (`card_json`, `csv`, `json_compact`, `json_records`), `MaxRows`, `NodeIds` and `Strict`. `Web` (`TakoWebSourceConfig`) has `Count` (1-20), `IncludeContents`, `Category` (`finance`, `news`, `sports`), `IncludeDomains`, `ExcludeDomains`, `SnippetMaxChars`, `Highlights`, `ArticleContentMaxChars`, `PublishedAfter` and `PublishedBefore`. With `IncludeContents` on a data source, each result adds a data surcharge: limit `Count` and `MaxRows` to control cost.
- **Location** _TakoLocation?_ - End-user `Latitude` and `Longitude` for localized results.
- **CountryCode**, **Locale**, **Timezone** _string?_ - ISO 3166-1 alpha-2 country code, BCP-47 locale and IANA timezone.
- **OutputSettings** _TakoOutputSettings?_ - `ImageDarkMode`, and `ForceRefresh` (instant effort only). They control card rendering.
- **IncludeRelated** _int?_ - Maximum related search suggestions to include (1-20).
- **VariableCost** _bool?_ - Serve data from sources priced per unit, such as web traffic and SEO. Each such card adds a charge, and the total per call is capped by `MaxVariableCostUsd`. The Gateway defaults it to `true`. Set it to `false` to opt out; those cards then come back as placeholders. Only searches that include the data index return these cards.
- **MaxVariableCostUsd** _double?_ - The most one search call can be charged in variable cost, in USD. The Gateway defaults it to `0.25`. Set `RemoveMaxVariableCostCap` to `true` to remove the cap (`maxVariableCostUsd: null`). Tako checks each card's maximum charge (`content.export_pricing.variable_cost_max_usd`) in rank order. A card that doesn't fit comes back as a placeholder with `reason: "over_cap"`, even if its actual charge would have fit. `0` returns only the cards whose charge the base price already covers. It can't be combined with `VariableCost = false`.
- **ExtraBody** _IDictionary&lt;string, JsonNode?&gt;_ - Extra Tako request fields, in snake_case, deep-merged into the request body last. Use it for Tako parameters this tool doesn't model yet. It can't set a field the tool already models (the gateway answers `invalid_input`), Tako rejects unknown keys (`api_error`), and it is limited to 16,384 serialized characters.

When Tako withholds a card's data, it returns a placeholder card instead. A placeholder keeps its title and description but carries no data. `placeholder.withheld_source` names the source, and `placeholder.reason` is `not_served` (variable cost is off, or the account isn't served that source) or `over_cap`. `usage.variable_cost.cost_usd` reports the variable-cost charge for the call.

The model fills the input schema (`GatewayTakoSearch.InputSchema`): `query`, plus `effort`, `sources`, `location`, `country_code`, `locale`, `timezone`, `output_settings` and `include_related` in snake_case.

Deserialize the tool output (`ExecutedTool.OutputJson`) into `TakoSearchOutput` to read it typed: `Response` is the `TakoSearchResponse`, and `Error` is a `GatewayToolError` (`Error` code, `Message`, `StatusCode`, `RetryAfter`, `StatusText`) when the tool failed. It holds `TakoCard`, `TakoWebResult` and `TakoResultContent`. Fields the schema doesn't declare are kept in `AdditionalProperties`.

```csharp
var output = JsonSerializer.Deserialize<TakoSearchOutput>(toolResult.OutputJson);
Console.WriteLine(output?.Response?.Usage?.VariableCost?.CostUsd);
```

`GatewayTakoSearch.OutputSchema` describes these fields, including `relevance_score`, `content.export_pricing.variable_cost_max_usd` and `usage.variable_cost`. `export_pricing.free_rows` is deprecated and optional: Tako always sends 0 and will stop sending it. `GatewayTakoSearch.ResultContentSchema` is the schema of a card or web result `content`, for reuse by other Tako tools.

## Preliminary tool results

A tool can stream its output. Set `Tool.ExecuteStream`, a delegate that takes the input and the `ToolExecutionOptions` of the call and returns an `IAsyncEnumerable<string>` of JSON outputs. It maps to an async-iterable `execute`. The loop calls it instead of `Execute` and `ExecuteWithOptions`.

```csharp
var cityAttractions = Tool.Function("cityAttractions", null, schema, null);
cityAttractions.ExecuteStream = (input, options) => Attractions(input.GetProperty("city").GetString()!);

static async IAsyncEnumerable<string> Attractions(string city)
{
    yield return JsonSerializer.Serialize(new { status = "loading", text = $"Getting attractions for {city}" });
    yield return JsonSerializer.Serialize(new { status = "success", text = $"Attractions in {city}" });
}
```

`StreamTextAsync` emits each streamed output as a `ToolResultPart` whose `ExecutedTool.Preliminary` is true, then one final `ToolResultPart` with the last output. In the UI message stream, a preliminary result is a `tool-output-available` chunk with `preliminary: true`. Only the final result reaches `StepResult.ToolResults`, the step content, and the model. `GenerateTextAsync` runs the tool the same way and keeps only the final result. A tool that yields nothing has the output `null`.

A provider-executed tool can stream preliminary results too, such as the partial images of OpenAI image generation. `ToolResultStreamPart.Preliminary` marks them, `StreamTextAsync` forwards the flag on the `ToolResultPart`, and only the final result reaches the step content. A `GeneratedToolResult` that a provider returns in the content of a model call can set `Preliminary` too, and the OpenTelemetry integration records only the final provider-executed result on the `execute_tool` span.

The approval flow of `streamText` is not ported yet ([#847](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/847)), so an approved call does not stream its preliminary results.

## Custom tools

A custom tool is a non-function tool that takes free-form text. OpenAI Responses supports it. Declare it with `OpenAITools.CustomTool(new OpenAICustomToolOptions { Name = ... })`. Add `GrammarSyntax` (`regex` or `lark`) and `GrammarDefinition` to constrain the input. The tool has no `Execute`. Run it yourself and send the result back in a `ToolModelMessage`.

```csharp
var writeSql = OpenAITools.CustomTool(new OpenAICustomToolOptions
{
    Name = "write_sql",
    Description = "Write a SQL SELECT query to answer the user question.",
    GrammarSyntax = "regex",
    GrammarDefinition = "SELECT .+",
});
```

When you send a custom tool call back in the next request, OpenAI Responses sends it as a `custom_tool_call` item. A string input is sent as is, and any other input as JSON. A call with an `itemId` in its OpenAI provider options is sent as an `item_reference` when the response is stored. An `async` option is kept. The result of the call is sent as `custom_tool_call_output`.

OpenAI Responses does not yet read `custom_tool_call` items in a response ([#571](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/571)), nor stream them ([#495](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/495)). Until then, a response that calls a custom tool does not return the call.

## Web fetch tool

`WebFetchTools.Create` (`Vercel.AI.WebFetch`) lets agents read web pages and documents by URL. It maps to `experimental_createWebFetchTool` of `@ai-sdk/tool-web-fetch` and is part of `Vercel.AI`.

```csharp
using Vercel.AI.WebFetch;

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Tools = new[] { WebFetchTools.Create() },
    StopWhen = StopWhen.IsStepCount(5),
    Prompt = "Summarize https://ai-sdk.dev/docs/introduction.",
});
```

`WebFetchTools.Create` takes an optional `WebFetchToolOptions` and returns the tool, named `webFetch`, ready for `Tools`.

| Option | Description |
| --- | --- |
| `AllowedDomains` | Only fetch from these domains and their subdomains, e.g. `new[] { "example.com" }`. List a subdomain such as `docs.example.com` to allow only that subdomain. Also applies to redirects. Cannot be combined with `BlockedDomains`. |
| `BlockedDomains` | Never fetch from these domains or their subdomains. Also applies to redirects. |
| `RequireUrlInUserMessage` | Only fetch URLs that appear in user messages. URLs in instructions, system messages, assistant messages, and tool results are not included. Defaults to `false`. |
| `MaxLength` | Maximum characters returned per call. Longer content can be read with `offset`. Defaults to `50000`. |
| `MaxBytes` | Maximum response size in bytes. Defaults to 10 MiB. |
| `Timeout` | Timeout for fetching each URL, including redirects. Defaults to 30 seconds. |
| `Fetch` | Custom `DownloadFetch`, for example to route requests through a proxy. It must validate resolved addresses itself. |
| `DefuddleOptions` | Options for the HTML content extractor, such as `ContentSelector`. |

An invalid option, or `AllowedDomains` together with `BlockedDomains`, throws an `InvalidArgumentError`.

The model calls the tool with `{ url: string, offset?: number }` and receives one of the following results:

- `{ type: 'document', url, finalUrl, contentType, title?, content, offset, totalLength, nextOffset? }` for HTML and text.
- `{ type: 'error', url, errorCode, message, statusCode? }` when fetching fails.

Errors are returned so the model can recover, for example by trying another URL. The model receives documents as text with the URL, title, and content type, followed by the content (`Tool.ToModelOutput`). Cancelling the `CancellationToken` aborts the request.

The tool runs with `Tool.ExecuteWithOptions`, which receives the `ToolExecutionOptions` of the call: its `ToolCallId`, the `Messages` of the step (the instructions are not part of them), and the `AbortSignal`. `Tool.ToModelOutput` converts the JSON output of a tool into the result sent to the model.

### Content handling

Requests prefer markdown using the `Accept: text/markdown` header, which many documentation sites support. Otherwise, HTML pages are converted to markdown, which removes navigation, footers, and other boilerplate. Text formats such as plain text, JSON, XML, and CSV are returned as is. PDFs, images, audio, video, and other binary formats are not supported.

JavaScript is not executed. Pages that render their content in the browser may return little or no content, in which case the model is told that the page may require JavaScript.

The port converts HTML with [AngleSharp](https://anglesharp.github.io/) and a built-in extractor instead of [defuddle](https://github.com/kepano/defuddle). The extractor sometimes selects the wrong part of a page. If you fetch pages with a known structure, set `DefuddleOptions.ContentSelector` to choose the main content element. `WebFetchDefuddleOptions` also has `RemoveImages`, `RemoveHiddenElements`, `RemoveExactSelectors` and `RemovePartialSelectors`. The last two use defuddle's lists of ad, social and navigation clutter selectors. The other defuddle options are not ported ([#756](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/756)).

```csharp
var webFetch = WebFetchTools.Create(new WebFetchToolOptions
{
    DefuddleOptions = new WebFetchDefuddleOptions { ContentSelector = "article" },
});
```

Content longer than `MaxLength` is truncated. The model is told to call the tool again with `offset` to continue reading. Converted documents are cached for a few minutes per tool instance, so reading at an offset does not fetch the page again.

### Security

The tool makes GET requests without cookies or credentials, and rejects URLs that contain credentials. Each request and redirect is validated against private and reserved addresses such as `localhost` and cloud metadata endpoints. The default transport does not validate resolved DNS addresses or pin connections, so pass a DNS-validating `Fetch` such as `GuardedFetch`, or restrict network egress at the platform level. A custom `Fetch` must provide equivalent validation.

`Download.FetchUntrustedUrlAsync` (`fetchUntrustedUrl` upstream) fetches a URL with the same validated redirects and first-hop credential isolation. Without a `credentialedOrigin` (or a `trustedOrigin`, when `credentialedOrigin` is omitted) that matches the URL, only allowlisted metadata headers (`accept`, `accept-language`, `range`, `user-agent`, conditional and tracing headers, and similar) go out on the first hop. Add non-credential names with `untrustedFirstHopHeaders`. Proxy, metadata, cookie, and hop-by-hop headers are removed even for a matching origin. Both origins must come from developer configuration, never from response data. A `credentialedOrigin` does not exempt the URL from validation, but a `trustedOrigin` does.

```csharp
var response = await Download.FetchUntrustedUrlAsync(
    fileUrl,
    headers,
    credentialedOrigin: "https://api.example.com",
    fetch: guardedFetch);
```

Fetched content is untrusted and can contain prompt injections. When an agent has access to private data, a model could also be instructed to send that data to another site, for example in a URL query string. Restrict fetching with `AllowedDomains`, enable `RequireUrlInUserMessage` so only URLs from user messages can be fetched, or require tool approval (`NeedsApproval`).

`RequireUrlInUserMessage` ignores URLs in tool results because the model can place arbitrary text there, for example through tool inputs that are reflected in outputs. As a result, the model cannot follow links from search results or fetched pages in this mode.

## MCP tool schemas and typed output

`McpClient.ConnectAsync` discovers every tool by default, and a tool returns the call-tool result as JSON text. Pass `McpClientOptions.Schemas` (or `McpClient.ListToolsAsync(transport, schemas)`) to list only the named tools with your own schemas, like `client.tools({ schemas })` upstream. An entry is an `McpToolSchema` with the `InputSchema` JSON Schema and an optional `OutputSchema`.

```csharp
var tools = await McpClient.ConnectAsync(
    transport,
    new McpClientOptions
    {
        Schemas = new Dictionary<string, McpToolSchema>
        {
            ["get-weather"] = new McpToolSchema(inputSchemaJson) { OutputSchema = outputSchemaJson },
        },
    }
);
```

With an `OutputSchema`, the tool returns the `structuredContent` of the result, validated against the schema. Without `structuredContent`, it parses the first text content as JSON and validates that. A mismatch, invalid JSON, or a result with neither throws an `MCPClientError`. A result with `isError` set is returned as is, with no validation. The schema is also set as `Tool.OutputSchema`.

## MCP transports

`McpClient.ConnectAsync(transport)` lists the tools of an MCP server over an `IMcpTransport`. The port ships `StdioMcpTransport`, `HttpMcpTransport` (Streamable HTTP), and `SseMcpTransport` (the legacy HTTP+SSE transport, `SseMCPTransport` upstream).

```csharp
using var transport = new SseMcpTransport(httpClient, new Uri("https://example.com/sse"), headers);
var tools = await McpClient.ConnectAsync(transport);
```

`SseMcpTransport` mirrors upstream's `start`, `send`, `close`, `onmessage`, `onerror`, `onclose`, and `protocolVersion` as `StartAsync`, `SendAsync`, `CloseAsync`, `OnMessage`, `OnError`, `OnClose`, and `ProtocolVersion`. `CallAsync` starts the transport when needed. The `fetch` option is the `HttpClient` you pass in.

A non-2xx response to the event stream throws an `MCPClientError` with `StatusCode`, `Url`, and `ResponseBody`. Its message is `MCP SSE Transport Error: {status} {reason}: {body}`, plus a hint to use the `http` transport on a 405. A non-2xx response to a `SendAsync` post throws the same error, with the message `MCP SSE Transport Error: POSTing to endpoint (HTTP {status}): {body}`. The `endpoint` event must stay on the origin of the event stream.

`HttpMcpTransport` throws an `MCPClientError` when the server answers a request with a JSON-RPC `error` body, even on a non-2xx response. The error carries `Code`, `Data`, `StatusCode`, `Url` and `ResponseBody`, and its message is the JSON-RPC `error.message`. Upstream also sets `cause` to the error object. The port's `Exception` cause cannot hold it.

To keep MCP calls on validated connections, pass the pinned client of a `GuardedFetch` as the `HttpClient`: `new HttpMcpTransport(guardedFetch.Client, endpoint)`. List the MCP server origin in `new GuardedFetch(trustedOrigins)` when it is a developer-configured private endpoint. This is the `fetch` option of the `http` transport upstream.

The `redirect` option and OAuth (`authProvider`) are not ported ([#585](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/585)). Redirects follow the `HttpClient` handler.

## Tool result title, metadata and dynamic flag

`ExecutedTool` carries `Title`, `ToolMetadata` and `Dynamic` from the tool call it answers, and `ToolInputStartPart` carries the tool's `Title`. `Dynamic` is true for a dynamic tool and for an invalid call. The `GeneratedToolResult` of a client tool in `Content` carries `Dynamic` and `ToolMetadata` too, and `ToolInputStartPart` also carries the tool's `ToolMetadata`. That result also sets `ProviderExecuted` to false (a provider's own result keeps it true) and carries the `InputJson` of the call it answers. It also has `Preliminary`, true for a result that a tool streamed before its final result. The response messages of a step (the assistant message and one tool message that carries the approval responses in `ToolModelMessage.ApprovalResponses` and the results in tool call order for `GenerateTextAsync` and in completion order for a streamed step; a denied call, provider-executed or not, gets an `execution-denied` result) are built from `Content` in one place, as upstream's `toResponseMessages` does. The assistant message now includes the step's files and skips empty text that has no provider metadata.

### Protocol version discovery

`HttpMcpTransport` implements `IMcpProtocolVersionDiscoveryTransport` (upstream's `supportsProtocolVersionDiscovery` and `waitsForProtocolVersionDiscovery`). `McpClient.ConnectAsync(transport, options)` first sends `server/discover` and falls back to `initialize` when the server does not know it. Set `McpClientOptions.ProtocolVersionDiscovery` to `false` (`protocolVersionDiscovery` upstream) for a legacy server that needs `initialize` first. The default is `true`.

A server that answers with `-32022` and lists only legacy versions in `data.supported` makes `ConnectAsync` throw an `MCPClientError` that explains the mismatch. It does not fall back to `initialize`. The error keeps `Code`, `Data`, `StatusCode` and `Url`, and its `InnerException` is the original error. A network error during discovery is rethrown too, with no fallback.

`IMcpProtocolVersionDiscoveryTransport.ProtocolVersion` (upstream's `protocolVersion`) is get-only, and the client writes the negotiated version through `SetProtocolVersion` (upstream's `setProtocolVersion`), so a transport can keep it in its own state. `HttpMcpTransport.ProtocolVersion` is sent as the `mcp-protocol-version` header. It starts as `McpClient.LatestLegacyProtocolVersion`. The client sets it to `McpClient.LatestProtocolVersion` before `server/discover` and back to the legacy version before `initialize`. A transport whose `WaitsForProtocolVersionDiscovery` is `false` gets a 1000ms limit for discovery. When it expires, `ConnectAsync` falls back to `initialize`. A slow response from a transport that waits is awaited with no limit.

## MCP client instance

`McpClient.CreateAsync(transport, options)` maps `createMCPClient`. It initializes the connection and returns a client that keeps what the server announced: `ServerInfo` (`serverInfo`), `InitializeResult` (`initializeResult`) and `Instructions` (`instructions`). `ListToolsAsync()` on the instance lists the tools, and `McpClient.ConnectAsync(transport)` stays as a shortcut for `CreateAsync` followed by `ListToolsAsync`.

After `server/discover` the client treats the server as modern. It adds `io.modelcontextprotocol/protocolVersion`, `io.modelcontextprotocol/clientCapabilities` and `io.modelcontextprotocol/clientInfo` to the `_meta` of every request, and throws an `MCPClientError` (`Modern MCP result is missing resultType`) when a result has no `resultType`. After `initialize` the client sets the negotiated protocol version on the transport and sends `notifications/initialized` through `IMcpNotificationTransport.NotifyAsync`. `StdioMcpTransport`, `HttpMcpTransport` and `SseMcpTransport` implement it. A transport that does not implement it gets no notification. `initialize` sends the client info `ai-sdk-mcp-client` `1.0.0`, like upstream's defaults.

An `initialize` result must carry `protocolVersion`, `capabilities` and `serverInfo`, and the version must be one the client supports. Otherwise `CreateAsync` throws an `MCPClientError`.

## MCP tool output

Tools from `McpClient` set `ToModelOutput`, which maps upstream's `mcpToModelOutput`. The output of an MCP tool is the JSON text of the call-tool result, and `ToModelOutput` converts it for the model:

- A JSON string becomes a `TextToolModelOutput`. An output that is not JSON at all (for example a plain string from a wrapper) is also text.
- A number, boolean, `null`, an empty or missing output, or an object without a `content` array becomes a `JsonToolModelOutput`. A missing output becomes JSON `null`.
- An object with a `content` array becomes a `ContentToolModelOutput`. A `text` part keeps its text. An MCP `image` part becomes a `FileToolModelContentPart` with the same media type and tagged inline base64 data (`FilePartInput.TaggedData(...)`). Any other part is sent as text holding its JSON.
- `FilePartInput.TaggedUrl(url, originalUrl)` is a tagged URL that keeps the string the `Uri` was parsed from in `OriginalUrl`, so a URL that the parser rewrites (an opaque URI such as `s3://bucket/key`) is sent as written. It maps to the `originalUrl` of upstream's `{ type: 'url', url, originalUrl }` file data.

Behavior change: `Tool.Execute` of an MCP tool used to return the joined text of the content parts (or the raw result when there was no text). Without an `OutputSchema` it now returns the normalized call-tool result as JSON, for example `{"content":[{"type":"text","text":"sunny"}],"isError":false}`, as upstream's `execute` returns the `CallToolResult`. Code that read the plain text from `Execute` must read `content[].text` instead. A result with `isError` is returned as that JSON in every schema mode, automatic or `schemas`, and does not throw, as upstream's `execute` returns the result.

## Sandbox

`ISandboxSession` (maps to `Experimental_SandboxSession`) is a session that runs commands (`RunAsync`, `SpawnAsync`) and reads and writes files (`ReadFileAsync`, `ReadBinaryFileAsync`, `ReadTextFileAsync`, `WriteFileAsync`, `WriteBinaryFileAsync`, `WriteTextFileAsync`, `StatAsync`, `ListDirectoryAsync`, `DeleteFileAsync`). Its `Description` can go into the instructions of an agent. You implement it for your sandbox provider.

Set `ExperimentalSandbox` on `GenerateTextOptions` or `StreamTextOptions` to pass a session to tool execution. A tool reads it as `ToolExecutionOptions.ExperimentalSandbox`. `Agent.GenerateAsync` and `Agent.Stream` take it as the `experimentalSandbox` parameter and pass it to `AgentOptions.PrepareCall` as `ExperimentalSandbox` on the options. `PrepareStepContext.ExperimentalSandbox` is the sandbox the step operates in. Return a `PrepareStepUpdate` with `ExperimentalSandbox` to change it for tool execution in that step only. Maps to `experimental_sandbox`.

A tool `DescriptionFactory` on `PromptTool` receives a `ToolDescriptionContext`. Its `Context` is the tool's context value and its `ExperimentalSandbox` is the active `ISandboxSession`, so a description can read the sandbox. `PromptTools.PrepareTools` takes the session as the `experimentalSandbox` parameter. Maps to the `experimental_sandbox` argument of `prepareTools`.

```csharp
var tool = new PromptTool("function")
{
    DescriptionFactory = context => "Env: " + (context.ExperimentalSandbox?.Description ?? "none"),
};
var prepared = PromptTools.PrepareTools(tools, experimentalSandbox: session);
```

**Breaking change:** `ToolDescriptionContext.SandboxDescription` and the `sandboxDescription` parameter of `PromptTools.PrepareTools` are removed. Pass the session as `experimentalSandbox` and read `context.ExperimentalSandbox?.Description`.

## Gateway DeepSeek search and fetch tools

`GatewayDeepSeekSearch.Create(config)` and `GatewayDeepSeekFetch.Create(config)` declare the AI Gateway's DeepSeek Search and Fetch tools. They are provider-defined tools (`gateway.deepseek_search` and `gateway.deepseek_fetch`) that the Gateway executes for any model, separate from native model web search. Search runs a web search through DeepSeek's standalone Search API. Fetch returns a page as Markdown.

```csharp
var search = GatewayDeepSeekSearch.Create(new GatewayDeepSeekSearchConfig { Count = 5, NeedContent = true });
var fetch = GatewayDeepSeekFetch.Create(new GatewayDeepSeekFetchConfig { Url = "https://example.com" });
```

`GatewayDeepSeekSearchConfig` is developer-only: the model can't see or set it, except where it pins an input.

- **Query** _string?_ - Pin the query instead of letting the model choose it.
- **Count** _int?_ - Maximum number of results, 1-50 (default 10).
- **NeedContent** _bool?_ - Include available page content (default `false`).
- **OriQuestion** _string?_ - Original question, 1-128 Unicode characters. Set `OriQuestionNull` to send `null` (no question).
- **Localization** _string?_ - IP address used to localize results. It is developer-only and not in the tool input schema.

`GatewayDeepSeekFetchConfig.Url` pins the target URL instead of letting the model choose it.

The model fills the input schema. Search takes `query`, `count`, `need_content` and `ori_question` in snake_case. Fetch takes `url` (at most 8192 characters, HTTP(S) only, no username or password). The Gateway validates the values, so the schemas do not limit them.

Deserialize a tool output into `DeepSeekSearchOutput` or `DeepSeekFetchOutput`. Each holds a `Response` (`DeepSeekSearchResponse` with `Results`, or `DeepSeekFetchResponse` with `Title` and Markdown `Content`) or an `Error` (`GatewayToolError`). Fields the schema does not declare stay in `AdditionalProperties`.

Search queries support the operators `site:`, `-site:`, `lang:`, `after:`, `before:` and quoted phrases, and must be 1-128 Unicode characters and not all whitespace. An empty `Results` list is a successful search with no matches. Result metadata (`Date`, `SiteName`, `SiteIcon` and so on) can be `null`. A tool failure comes back as a `tool-error` part in the step content, not in the tool results.
