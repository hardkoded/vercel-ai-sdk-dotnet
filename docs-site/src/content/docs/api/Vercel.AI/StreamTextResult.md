---
title: "StreamTextResult"
description: "Result of streamText. Enumerating Stream(CancellationToken) or awaiting Text consumes the same buffered events."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextResult"
  hidden: true
---

<a id="Vercel_AI_StreamTextResult"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class StreamTextResult
```


Result of `streamText`. Enumerating [Stream(CancellationToken)](#Vercel_AI_StreamTextResult_Stream_System_Threading_CancellationToken_) or awaiting [Text](#Vercel_AI_StreamTextResult_Text) consumes the same buffered events.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [StreamTextResult&lt;TOutput&gt;](/api/Vercel.AI/StreamTextResult-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5287)

## Properties

<a id="Vercel_AI_StreamTextResult_Content_"></a>
<a id="Vercel_AI_StreamTextResult_Content"></a>

### Content


```csharp
public Task<IReadOnlyList<GeneratedContent>> Content { get; }
```


Content from every step. See [Content](/api/Vercel.AI/StepResult/#Vercel_AI_StepResult_Content). Completes when the stream finishes. Maps to `content`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedContent&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5723)

<a id="Vercel_AI_StreamTextResult_DynamicToolCalls_"></a>
<a id="Vercel_AI_StreamTextResult_DynamicToolCalls"></a>

### DynamicToolCalls


```csharp
public Task<IReadOnlyList<GeneratedToolCall>> DynamicToolCalls { get; }
```


Dynamic tool calls from every step. Completes when the stream finishes. Maps to `dynamicToolCalls`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedToolCall&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5744)

<a id="Vercel_AI_StreamTextResult_DynamicToolResults_"></a>
<a id="Vercel_AI_StreamTextResult_DynamicToolResults"></a>

### DynamicToolResults


```csharp
public Task<IReadOnlyList<ExecutedTool>> DynamicToolResults { get; }
```


Dynamic tool results from every step. Completes when the stream finishes. Maps to `dynamicToolResults`.

**Value:** [Task&lt;IReadOnlyList&lt;ExecutedTool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5753)

<a id="Vercel_AI_StreamTextResult_Files_"></a>
<a id="Vercel_AI_StreamTextResult_Files"></a>

### Files


```csharp
public Task<IReadOnlyList<GeneratedFile>> Files { get; }
```


Files generated in every step. Completes when the stream finishes. Maps to `files`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedFile&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5717)

<a id="Vercel_AI_StreamTextResult_FinalStep_"></a>
<a id="Vercel_AI_StreamTextResult_FinalStep"></a>

### FinalStep


```csharp
public Task<StepResult> FinalStep { get; }
```


The last completed step. Completes when the stream finishes. Maps to `finalStep`.

**Value:** [Task&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5720)

<a id="Vercel_AI_StreamTextResult_FinishReason_"></a>
<a id="Vercel_AI_StreamTextResult_FinishReason"></a>

### FinishReason


```csharp
public Task<FinishReason> FinishReason { get; }
```


Finish reason. Completes when the stream finishes.

**Value:** [Task&lt;FinishReason&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5705)

<a id="Vercel_AI_StreamTextResult_Output_"></a>
<a id="Vercel_AI_StreamTextResult_Output"></a>

### Output


```csharp
public Task<JsonElement?> Output { get; }
```


The parsed structured output, or null when no output is set. Completes when the stream finishes. Maps to `output`.

**Value:** [Task&lt;JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5699)

<a id="Vercel_AI_StreamTextResult_ProviderMetadata_"></a>
<a id="Vercel_AI_StreamTextResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public Task<JsonElement?> ProviderMetadata { get; }
```


Provider metadata of the last step. Completes when the stream finishes. Maps to `providerMetadata`.

**Value:** [Task&lt;JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5702)

<a id="Vercel_AI_StreamTextResult_RawFinishReason_"></a>
<a id="Vercel_AI_StreamTextResult_RawFinishReason"></a>

### RawFinishReason


```csharp
public Task<string?> RawFinishReason { get; }
```


The finish reason exactly as the provider reported it, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when it reported none. Completes when the stream finishes. Maps to `rawFinishReason`.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5708)

<a id="Vercel_AI_StreamTextResult_Reasoning_"></a>
<a id="Vercel_AI_StreamTextResult_Reasoning"></a>

### Reasoning


```csharp
public Task<IReadOnlyList<GeneratedContent>> Reasoning { get; }
```


The reasoning parts and reasoning files of the last step. Completes when the stream finishes. Maps to `reasoning`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedContent&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5732)

<a id="Vercel_AI_StreamTextResult_ReasoningText_"></a>
<a id="Vercel_AI_StreamTextResult_ReasoningText"></a>

### ReasoningText


```csharp
public Task<string?> ReasoningText { get; }
```


Reasoning text of the last step. Completes when the stream finishes. Maps to `reasoningText`.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5729)

<a id="Vercel_AI_StreamTextResult_Request_"></a>
<a id="Vercel_AI_StreamTextResult_Request"></a>

### Request


```csharp
public Task<LanguageModelRequestMetadata> Request { get; }
```


Additional request information of the last step. [Body](/api/Vercel.AI/LanguageModelRequestMetadata/#Vercel_AI_LanguageModelRequestMetadata_Body) and
[Messages](/api/Vercel.AI/LanguageModelRequestMetadata/#Vercel_AI_LanguageModelRequestMetadata_Messages) are set only when [Include](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Include) asks for them.
Completes when the stream finishes. Maps to `request`.

**Value:** [Task&lt;LanguageModelRequestMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5760)

<a id="Vercel_AI_StreamTextResult_Response_"></a>
<a id="Vercel_AI_StreamTextResult_Response"></a>

### Response


```csharp
public Task<StepResponse> Response { get; }
```


Additional response information of the last step, with the messages that step added. Completes when the stream finishes. Maps to `response`.

**Value:** [Task&lt;StepResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5763)

<a id="Vercel_AI_StreamTextResult_ResponseMessages_"></a>
<a id="Vercel_AI_StreamTextResult_ResponseMessages"></a>

### ResponseMessages


```csharp
public Task<IReadOnlyList<ModelMessage>> ResponseMessages { get; }
```


Messages this call added to the conversation: the assistant message of each step, with any tool approval request, and the
tool results the client ran. Completes when the stream finishes. Maps to `responseMessages`.

**Value:** [Task&lt;IReadOnlyList&lt;ModelMessage&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5769)

<a id="Vercel_AI_StreamTextResult_Sources_"></a>
<a id="Vercel_AI_StreamTextResult_Sources"></a>

### Sources


```csharp
public Task<IReadOnlyList<GeneratedSource>> Sources { get; }
```


Sources from every step. Completes when the stream finishes. Maps to `sources`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedSource&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5735)

<a id="Vercel_AI_StreamTextResult_StaticToolCalls_"></a>
<a id="Vercel_AI_StreamTextResult_StaticToolCalls"></a>

### StaticToolCalls


```csharp
public Task<IReadOnlyList<GeneratedToolCall>> StaticToolCalls { get; }
```


Static tool calls from every step. Completes when the stream finishes. Maps to `staticToolCalls`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedToolCall&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5741)

<a id="Vercel_AI_StreamTextResult_StaticToolResults_"></a>
<a id="Vercel_AI_StreamTextResult_StaticToolResults"></a>

### StaticToolResults


```csharp
public Task<IReadOnlyList<ExecutedTool>> StaticToolResults { get; }
```


Static tool results from every step. Completes when the stream finishes. Maps to `staticToolResults`.

**Value:** [Task&lt;IReadOnlyList&lt;ExecutedTool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5750)

<a id="Vercel_AI_StreamTextResult_Steps_"></a>
<a id="Vercel_AI_StreamTextResult_Steps"></a>

### Steps


```csharp
public Task<IReadOnlyList<StepResult>> Steps { get; }
```


Completed steps. Completes when the stream finishes.

**Value:** [Task&lt;IReadOnlyList&lt;StepResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5714)

<a id="Vercel_AI_StreamTextResult_Text_"></a>
<a id="Vercel_AI_StreamTextResult_Text"></a>

### Text


```csharp
public Task<string> Text { get; }
```


Full text from the last step. Completes when the stream finishes.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5696)

<a id="Vercel_AI_StreamTextResult_ToolCalls_"></a>
<a id="Vercel_AI_StreamTextResult_ToolCalls"></a>

### ToolCalls


```csharp
public Task<IReadOnlyList<GeneratedToolCall>> ToolCalls { get; }
```


Tool calls from every step. Completes when the stream finishes. Maps to `toolCalls`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedToolCall&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5738)

<a id="Vercel_AI_StreamTextResult_ToolResults_"></a>
<a id="Vercel_AI_StreamTextResult_ToolResults"></a>

### ToolResults


```csharp
public Task<IReadOnlyList<ExecutedTool>> ToolResults { get; }
```


Tool results from every step. Completes when the stream finishes. Maps to `toolResults`.

**Value:** [Task&lt;IReadOnlyList&lt;ExecutedTool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5747)

<a id="Vercel_AI_StreamTextResult_Usage_"></a>
<a id="Vercel_AI_StreamTextResult_Usage"></a>

### Usage


```csharp
public Task<LanguageModelUsage> Usage { get; }
```


Total usage. Completes when the stream finishes.

**Value:** [Task&lt;LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5711)

<a id="Vercel_AI_StreamTextResult_Warnings_"></a>
<a id="Vercel_AI_StreamTextResult_Warnings"></a>

### Warnings


```csharp
public Task<IReadOnlyList<CallWarning>> Warnings { get; }
```


Warnings from every step. Completes when the stream finishes. Maps to `warnings`.

**Value:** [Task&lt;IReadOnlyList&lt;CallWarning&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5726)

## Methods

<a id="Vercel_AI_StreamTextResult_ConsumeStreamAsync_"></a>
<a id="Vercel_AI_StreamTextResult_ConsumeStreamAsync_System_Threading_CancellationToken_"></a>

### ConsumeStreamAsync(CancellationToken)


```csharp
public Task ConsumeStreamAsync(CancellationToken cancellationToken = default)
```


Consumes the stream without processing the parts. Use it when only the final result or the callbacks matter. Maps to `consumeStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5467)

<a id="Vercel_AI_StreamTextResult_ElementStream_"></a>
<a id="Vercel_AI_StreamTextResult_ElementStream_System_Threading_CancellationToken_"></a>

### ElementStream(CancellationToken)


```csharp
public IAsyncEnumerable<JsonElement> ElementStream(CancellationToken cancellationToken = default)
```


The elements of an array output as each one completes. Empty for any other output. The stream fails with a
type validation error when the model exceeds `maxItems`. Maps to `elementStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5581)

<a id="Vercel_AI_StreamTextResult_ExperimentalPartialOutputStream_"></a>
<a id="Vercel_AI_StreamTextResult_ExperimentalPartialOutputStream_System_Threading_CancellationToken_"></a>

### ExperimentalPartialOutputStream(CancellationToken)


```csharp
[Obsolete("Use PartialOutputStream instead.")]
public IAsyncEnumerable<JsonElement> ExperimentalPartialOutputStream(CancellationToken cancellationToken = default)
```


The deprecated alias of [PartialOutputStream(CancellationToken)](#Vercel_AI_StreamTextResult_PartialOutputStream_System_Threading_CancellationToken_). Maps to `experimental_partialOutputStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5556)

<a id="Vercel_AI_StreamTextResult_PartialOutputStream_"></a>
<a id="Vercel_AI_StreamTextResult_PartialOutputStream_System_Threading_CancellationToken_"></a>

### PartialOutputStream(CancellationToken)


```csharp
public IAsyncEnumerable<JsonElement> PartialOutputStream(CancellationToken cancellationToken = default)
```


The partial structured output after each text delta that changes it: the text so far for text output, the repaired partial
object for object output, the complete elements for array output, the choice for choice output, and the parsed JSON
so far for JSON output. Each step starts over. Maps to `partialOutputStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5539)

<a id="Vercel_AI_StreamTextResult_PipeTextStreamToResponseAsync_"></a>
<a id="Vercel_AI_StreamTextResult_PipeTextStreamToResponseAsync_Vercel_AI_Util_ServerResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_"></a>

### PipeTextStreamToResponseAsync(ServerResponse, int?, string?, HeaderCollection?, CancellationToken)


```csharp
public Task PipeTextStreamToResponseAsync(ServerResponse response, int? status = null, string? statusText = null, HeaderCollection? headers = null, CancellationToken cancellationToken = default)
```


Writes the text deltas to &lt;code class="paramref"&gt;response&lt;/code&gt; as `text/plain`. Maps to `pipeTextStreamToResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5492)

<a id="Vercel_AI_StreamTextResult_Stream_"></a>
<a id="Vercel_AI_StreamTextResult_Stream_System_Threading_CancellationToken_"></a>

### Stream(CancellationToken)


```csharp
public IAsyncEnumerable<TextStreamPart> Stream(CancellationToken cancellationToken = default)
```


Every stream part, including tool calls, tool results, and finish.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;TextStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5689)

<a id="Vercel_AI_StreamTextResult_TextStream_"></a>
<a id="Vercel_AI_StreamTextResult_TextStream_System_Threading_CancellationToken_"></a>

### TextStream(CancellationToken)


```csharp
public IAsyncEnumerable<string> TextStream(CancellationToken cancellationToken = default)
```


Text deltas only.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5515)

<a id="Vercel_AI_StreamTextResult_ToTextStreamResponse_"></a>
<a id="Vercel_AI_StreamTextResult_ToTextStreamResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_"></a>

### ToTextStreamResponse(int?, string?, HeaderCollection?, CancellationToken)


```csharp
public TextStreamResponse ToTextStreamResponse(int? status = null, string? statusText = null, HeaderCollection? headers = null, CancellationToken cancellationToken = default)
```


Builds a `text/plain` response of the text deltas. Each delta is its own UTF-8 body chunk. Maps to `toTextStreamResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [TextStreamResponse](/api/Vercel.AI.Util/TextStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5476)
