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

`GenerateTextOptions.Instructions`, `StreamTextOptions.Instructions`, and `AgentOptions.Instructions` take an `Instructions` value (`Vercel.AI.Prompt`): a string, one `SystemModelMessage`, or an array of `SystemModelMessage`. Each converts implicitly, so `Instructions = "Be brief."` still works. Use a system message to attach provider options to the instructions. An array becomes several leading system messages, in order. `Instructions` wins over `System`. Code that reads the property now gets an `Instructions`: use `Text` for a string and `Messages` for system messages. `ToolCallRepairContext.Instructions` and `System` are `Instructions` too, so a repair function reads `.Text` or `.Messages` there as well. The string conversion rejects null: assigning a null `string?` variable to `Instructions` throws `ArgumentNullException`. Leave `Instructions` unset when there are none.

Set `AgentOptions.PrepareCall` to change the options of each `Agent.GenerateAsync` and `Agent.Stream` call. It receives the options the agent would use, including the default `StopWhen`, and returns replacement options (or null to keep them). Returning them unchanged keeps the default warning; setting `StopWhen` replaces the default limit and silences it.

A call to a tool that has no `Execute` ends the loop at that step. The stop condition is not evaluated. The same holds for a tool with `NeedsApproval = true` when `ApproveTool` is not set: the call is not executed and no warning is logged. When `ApproveTool` is set, it decides instead.

Set `ApproveTool` to veto a call before it runs. A denied call is stored as an error tool result.

## Tool choice none

When `ToolChoice` is `ToolChoice.None`, the SDK prevents local tool execution even if the model returns a tool call. Such a response throws `ToolChoiceViolationException`. The tool definitions are still sent to the model. With `StreamTextAsync`, tool parts are withheld from the stream, so client-side tools are not invoked either. Tool call repair (`RepairToolCall`) and input refinement (`RefineToolInput`) are skipped for rejected tool calls. A `PrepareStep` that returns another `ToolChoice` replaces `None` for that step. The Hugging Face Responses provider sends `tool_choice: "none"`.

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

A step-start part ends a block. Each block becomes an assistant message, followed by the approval responses and tool results of its tools, each as its own tool message. Assistant `FileUIPart` and `ReasoningFileUIPart` parts are dropped, and the approval request keeps only its id, tool call id, and `IsAutomatic` ([#575](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/575)).

## Tool errors

A tool result carries an output type: `text`, `json`, `error-text`, `error-json`, or `execution-denied`. `ToolModelMessage.OutputType` holds it. When you do not set it, the type comes from `isError` and from whether the output is valid JSON.

Providers keep the error status:

- OpenAI Chat Completions and Responses send `error-text` and `error-json` as `{"error": <value>}`. `execution-denied` stays the reason, or `Tool call execution denied.` when there is none.
- OpenAI Responses honors `promptCacheBreakpoint` on a scalar tool result (`text`, `json`, `error-text`, `error-json`, `execution-denied`). Set it in the provider options of the `ToolModelMessage` (`ProviderMetadata`) or of its output (`OutputProviderOptions`); the output's wins. The result is then sent as an `input_text` array with `prompt_cache_breakpoint`.
- A `LanguageModelTool` with `OutputSchema` is sent to OpenAI Responses as `output_schema`. Its `text` and `execution-denied` results are then sent as JSON string literals, because OpenAI parses the result as JSON.
- Google sends `error-text`, `error-json`, and `execution-denied` on `functionResponse.response.error`. Successful results stay on `response.content`.

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

A provider tool declared with an input schema (the four-argument `Tool.ProviderDefined`) validates the input of its provider-executed calls, after it drops the properties the schema does not declare (it keeps them when `additionalProperties` is `true` or a schema). A call that does not match comes back in `Content` with `Invalid`, `Dynamic` and `Error` (an `InvalidToolInputError`) set. The provider already ran it, so the loop adds no client tool error and keeps the provider's own result.

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

The tool runs with `Tool.ExecuteWithOptions`, which receives the `ToolExecutionOptions` of the call: its `ToolCallId`, the `Messages` of the step, and the `AbortSignal`. `Tool.ToModelOutput` converts the JSON output of a tool into the result sent to the model.

### Content handling

Requests prefer markdown using the `Accept: text/markdown` header, which many documentation sites support. Otherwise, HTML pages are converted to markdown, which removes navigation, footers, and other boilerplate. Text formats such as plain text, JSON, XML, and CSV are returned as is. PDFs, images, audio, video, and other binary formats are not supported.

JavaScript is not executed. Pages that render their content in the browser may return little or no content, in which case the model is told that the page may require JavaScript.

The port converts HTML with [AngleSharp](https://anglesharp.github.io/) and a built-in extractor instead of [defuddle](https://github.com/kepano/defuddle). The extractor sometimes selects the wrong part of a page. If you fetch pages with a known structure, set `DefuddleOptions.ContentSelector` to choose the main content element. `WebFetchDefuddleOptions` also has `RemoveImages` and `RemoveHiddenElements`; the other defuddle options are not ported ([#756](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/756)).

```csharp
var webFetch = WebFetchTools.Create(new WebFetchToolOptions
{
    DefuddleOptions = new WebFetchDefuddleOptions { ContentSelector = "article" },
});
```

Content longer than `MaxLength` is truncated. The model is told to call the tool again with `offset` to continue reading. Converted documents are cached for a few minutes per tool instance, so reading at an offset does not fetch the page again.

### Security

The tool makes GET requests without cookies or credentials, and rejects URLs that contain credentials. Each request and redirect is validated against private and reserved addresses such as `localhost` and cloud metadata endpoints. On `net10.0` the default transport also validates the resolved DNS addresses and connects to the validated address, so a hostname that resolves to a private or reserved address returns `url_not_allowed`. On `netstandard2.0` it validates the resolved addresses just before the request but cannot pin the connection, so restrict network egress at the platform level. A custom `Fetch` must provide equivalent validation. See [Secure URL fetching](secure-url-fetching.md).

Fetched content is untrusted and can contain prompt injections. When an agent has access to private data, a model could also be instructed to send that data to another site, for example in a URL query string. Restrict fetching with `AllowedDomains`, enable `RequireUrlInUserMessage` so only URLs from user messages can be fetched, or require tool approval (`NeedsApproval`).

`RequireUrlInUserMessage` ignores URLs in tool results because the model can place arbitrary text there, for example through tool inputs that are reflected in outputs. As a result, the model cannot follow links from search results or fetched pages in this mode.

## MCP transports

`McpClient.ConnectAsync(transport)` lists the tools of an MCP server over an `IMcpTransport`. The port ships `StdioMcpTransport`, `HttpMcpTransport` (Streamable HTTP), and `SseMcpTransport` (the legacy HTTP+SSE transport, `SseMCPTransport` upstream).

```csharp
using var transport = new SseMcpTransport(httpClient, new Uri("https://example.com/sse"), headers);
var tools = await McpClient.ConnectAsync(transport);
```

`SseMcpTransport` mirrors upstream's `start`, `send`, `close`, `onmessage`, `onerror`, `onclose`, and `protocolVersion` as `StartAsync`, `SendAsync`, `CloseAsync`, `OnMessage`, `OnError`, `OnClose`, and `ProtocolVersion`. `CallAsync` starts the transport when needed. The `fetch` option is the `HttpClient` you pass in.

A non-2xx response to the event stream throws an `MCPClientError` with `StatusCode`, `Url`, and `ResponseBody`. Its message is `MCP SSE Transport Error: {status} {reason}: {body}`, plus a hint to use the `http` transport on a 405. A non-2xx response to a `SendAsync` post throws the same error, with the message `MCP SSE Transport Error: POSTing to endpoint (HTTP {status}): {body}`. The `endpoint` event must stay on the origin of the event stream.

The `redirect` option and OAuth (`authProvider`) are not ported ([#585](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/585)). Redirects follow the `HttpClient` handler.

## Tool result title, metadata and dynamic flag

`ExecutedTool` carries `Title`, `ToolMetadata` and `Dynamic` from the tool call it answers, and `ToolInputStartPart` carries the tool's `Title`. `Dynamic` is true for a dynamic tool and for an invalid call. The `GeneratedToolResult` of a client tool in `Content` carries `Dynamic` and `ToolMetadata` too, and `ToolInputStartPart` also carries the tool's `ToolMetadata`.

## MCP tool output

Tools from `McpClient` set `ToModelOutput`, which maps upstream's `mcpToModelOutput`. The output of an MCP tool is the JSON text of the call-tool result, and `ToModelOutput` converts it for the model:

- A JSON string becomes a `TextToolModelOutput`.
- A number, boolean, `null`, an empty or missing output, or an object without a `content` array becomes a `JsonToolModelOutput`. A missing output becomes JSON `null`.
- An object with a `content` array becomes a `ContentToolModelOutput`. A `text` part keeps its text, and any other part is sent as text holding its JSON.

Behavior change: `Tool.Execute` of an MCP tool used to return the joined text of the content parts (or the raw result when there was no text). It now returns the normalized call-tool result as JSON, for example `{"content":[{"type":"text","text":"sunny"}],"isError":false}`, as upstream's `execute` returns the `CallToolResult`. Code that read the plain text from `Execute` must read `content[].text` instead. A result with `isError` still throws an `AiSdkException`.

Not ported: `outputSchema` support ([#774](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/777)) and file parts for `image` content ([#775](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/778)).
