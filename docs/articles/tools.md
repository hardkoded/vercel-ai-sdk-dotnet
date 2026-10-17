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
    });

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Prompt = "Weather in Lisbon?",
    Tools = new[] { weather },
    StopWhen = StopWhen.IsStepCount(4),
});
```
<sup><a href='/examples/Tools/Program.cs#L15-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-tools' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The default stop condition is one step. That step’s tool calls still run, and the loop stops before a second model call. Raise `IsStepCount`, or use `HasToolCall` / `IsLoopFinished`, when the model should see the tool result.

`Agent` stops after 10 steps when `AgentOptions.StopWhen` is null. When that default limit is what stopped the loop, the agent logs one warning through `LogWarnings` (type `other`, with the provider and model of the last step). Set `StopWhen` explicitly, with `IsStepCount` or a custom condition, to raise the limit and silence it. `GenerateTextAsync` and `StreamTextAsync` do not log. `LogWarnings.Logger = false` suppresses the warning. `StepResult.Provider` and `StepResult.ModelId` name the model that produced each step.

Set `AgentOptions.PrepareCall` to change the options of each `Agent.GenerateAsync` and `Agent.Stream` call. It receives the options the agent would use, including the default `StopWhen`, and returns replacement options (or null to keep them). Returning them unchanged keeps the default warning; setting `StopWhen` replaces the default limit and silences it.

A call to a tool that has no `Execute` ends the loop at that step. The stop condition is not evaluated. The same holds for a tool with `NeedsApproval = true` when `ApproveTool` is not set: the call is not executed and no warning is logged. When `ApproveTool` is set, it decides instead.

Set `ApproveTool` to veto a call before it runs. A denied call is stored as an error tool result.

## Provider-executed tool approval

A provider-executed tool, such as an MCP tool, can ask for approval before the provider runs it. The model returns a `GeneratedToolApprovalRequest` (`tool-approval-request`) next to the tool call. The step keeps it in `Content` with its `ToolCall` filled in, and `StreamText` emits a `ToolApprovalRequestPart`. The step ends there, because there is no client tool call to run, and the finish reason stays `tool-calls`.

`GenerateTextResult.ResponseMessages` holds the messages the call added. The assistant message keeps an `AssistantToolApprovalRequestPart` after the tool call. To continue, send the history back with a `ToolApprovalResponseModelMessage` that has `ProviderExecuted = true`:

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

- `Reasoning`: `PruneReasoning.None` (default), `All`, or `BeforeLastMessage`, which keeps the reasoning of the last message.
- `ToolCalls`: a list of rules. `PruneToolCallsRule.All()`, `BeforeLastMessage()`, and `BeforeLastMessages(n)` take an optional list of tool names. Without names a rule prunes every tool. Calls, results, and approvals in the kept trailing messages stay, and so does the originating tool call of a kept approval response. An approval response whose request is unknown is pruned when the rule names tools.
- `EmptyMessages`: `PruneEmptyMessages.Remove` (default) drops messages that are empty after pruning, `Keep` keeps them.

Consecutive tool messages (results and approval responses) count as one message, as upstream's tool message holds several parts. Reasoning files are not supported (([#575](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/575))).

## Tool errors

A tool result carries an output type: `text`, `json`, `error-text`, `error-json`, or `execution-denied`. `ToolModelMessage.OutputType` holds it. When you do not set it, the type comes from `isError` and from whether the output is valid JSON.

Providers keep the error status:

- OpenAI Chat Completions and Responses send `error-text` and `error-json` as `{"error": <value>}`. `execution-denied` stays the reason, or `Tool call execution denied.` when there is none.
- OpenAI Responses honors `promptCacheBreakpoint` on a scalar tool result (`text`, `json`, `error-text`, `error-json`, `execution-denied`). Set it in the provider options of the `ToolModelMessage` (`ProviderMetadata`) or of its output (`OutputProviderOptions`); the output's wins. The result is then sent as an `input_text` array with `prompt_cache_breakpoint`.
- A `LanguageModelTool` with `OutputSchema` is sent to OpenAI Responses as `output_schema`. Its `text` and `execution-denied` results are then sent as JSON string literals, because OpenAI parses the result as JSON.
- Google sends `error-text`, `error-json`, and `execution-denied` on `functionResponse.response.error`. Successful results stay on `response.content`.

## Tool input validation

Before the loop runs a tool, `ToolCallParser.ParseToolCall` parses the call's input and validates it against the tool's `InputSchema`. Empty input counts as `{}`. A call whose input is not valid JSON or does not match the schema is not executed. It comes back with `Invalid` set and an `InvalidToolInputError` in `Error`, and the model receives the error message as an `error-text` tool result. The validator covers `type`, `enum`, `const`, `properties`, `required`, `additionalProperties`, `items`, `minItems`, `maxItems`, `minLength`, `maxLength`, `minimum`, `maximum`, `anyOf`, `oneOf` and `allOf`.

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

In the UI message stream, provider-executed chunks are marked `providerExecuted`, and an error chunk keeps the provider's error text or JSON instead of the `onError` text. A provider-executed tool approval request (MCP) is not supported ([#542](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/542)).

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
