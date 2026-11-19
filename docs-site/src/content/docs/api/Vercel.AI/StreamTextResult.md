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
public sealed class StreamTextResult
```


Result of `streamText`. Enumerating [Stream(CancellationToken)](#Vercel_AI_StreamTextResult_Stream_System_Threading_CancellationToken_) or awaiting [Text](#Vercel_AI_StreamTextResult_Text) consumes the same buffered events.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4156)

## Properties

<a id="Vercel_AI_StreamTextResult_Files_"></a>
<a id="Vercel_AI_StreamTextResult_Files"></a>

### Files


```csharp
public Task<IReadOnlyList<GeneratedFile>> Files { get; }
```


Files generated in the last step. Completes when the stream finishes. Maps to `files`.

**Value:** [Task&lt;IReadOnlyList&lt;GeneratedFile&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4418)

<a id="Vercel_AI_StreamTextResult_FinishReason_"></a>
<a id="Vercel_AI_StreamTextResult_FinishReason"></a>

### FinishReason


```csharp
public Task<FinishReason> FinishReason { get; }
```


Finish reason. Completes when the stream finishes.

**Value:** [Task&lt;FinishReason&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4409)

<a id="Vercel_AI_StreamTextResult_Output_"></a>
<a id="Vercel_AI_StreamTextResult_Output"></a>

### Output


```csharp
public Task<JsonElement?> Output { get; }
```


The parsed structured output, or null when no output is set. Completes when the stream finishes. Maps to `output`.

**Value:** [Task&lt;JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4403)

<a id="Vercel_AI_StreamTextResult_ProviderMetadata_"></a>
<a id="Vercel_AI_StreamTextResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public Task<JsonElement?> ProviderMetadata { get; }
```


Provider metadata of the last step. Completes when the stream finishes. Maps to `providerMetadata`.

**Value:** [Task&lt;JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4406)

<a id="Vercel_AI_StreamTextResult_Request_"></a>
<a id="Vercel_AI_StreamTextResult_Request"></a>

### Request


```csharp
public Task<LanguageModelRequestMetadata> Request { get; }
```


Additional request information of the last step. [Body](/api/Vercel.AI/LanguageModelRequestMetadata/#Vercel_AI_LanguageModelRequestMetadata_Body) and
[Messages](/api/Vercel.AI/LanguageModelRequestMetadata/#Vercel_AI_LanguageModelRequestMetadata_Messages) are set only when [Include](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_Include) asks for them.
Completes when the stream finishes. Maps to `request`.

**Value:** [Task&lt;LanguageModelRequestMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4425)

<a id="Vercel_AI_StreamTextResult_Response_"></a>
<a id="Vercel_AI_StreamTextResult_Response"></a>

### Response


```csharp
public Task<StepResponse> Response { get; }
```


Additional response information of the last step, with the messages that step added. Completes when the stream finishes. Maps to `response`.

**Value:** [Task&lt;StepResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4428)

<a id="Vercel_AI_StreamTextResult_ResponseMessages_"></a>
<a id="Vercel_AI_StreamTextResult_ResponseMessages"></a>

### ResponseMessages


```csharp
public Task<IReadOnlyList<ModelMessage>> ResponseMessages { get; }
```


Messages this call added to the conversation: the assistant message of each step, with any tool approval request, and the
tool results the client ran. Completes when the stream finishes. Maps to `responseMessages`.

**Value:** [Task&lt;IReadOnlyList&lt;ModelMessage&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4434)

<a id="Vercel_AI_StreamTextResult_Steps_"></a>
<a id="Vercel_AI_StreamTextResult_Steps"></a>

### Steps


```csharp
public Task<IReadOnlyList<StepResult>> Steps { get; }
```


Completed steps. Completes when the stream finishes.

**Value:** [Task&lt;IReadOnlyList&lt;StepResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4415)

<a id="Vercel_AI_StreamTextResult_Text_"></a>
<a id="Vercel_AI_StreamTextResult_Text"></a>

### Text


```csharp
public Task<string> Text { get; }
```


Full text from the last step. Completes when the stream finishes.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4400)

<a id="Vercel_AI_StreamTextResult_Usage_"></a>
<a id="Vercel_AI_StreamTextResult_Usage"></a>

### Usage


```csharp
public Task<LanguageModelUsage> Usage { get; }
```


Total usage. Completes when the stream finishes.

**Value:** [Task&lt;LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4412)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4237)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4310)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4285)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4268)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4393)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4244)
