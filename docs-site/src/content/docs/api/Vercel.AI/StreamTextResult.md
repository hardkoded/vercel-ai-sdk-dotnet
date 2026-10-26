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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L1974)

## Properties

<a id="Vercel_AI_StreamTextResult_FinishReason_"></a>
<a id="Vercel_AI_StreamTextResult_FinishReason"></a>

### FinishReason


```csharp
public Task<FinishReason> FinishReason { get; }
```


Finish reason. Completes when the stream finishes.

**Value:** [Task&lt;FinishReason&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2026)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2039)

<a id="Vercel_AI_StreamTextResult_Response_"></a>
<a id="Vercel_AI_StreamTextResult_Response"></a>

### Response


```csharp
public Task<StepResponse> Response { get; }
```


Additional response information of the last step, with the messages that step added. Completes when the stream finishes. Maps to `response`.

**Value:** [Task&lt;StepResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2042)

<a id="Vercel_AI_StreamTextResult_Steps_"></a>
<a id="Vercel_AI_StreamTextResult_Steps"></a>

### Steps


```csharp
public Task<IReadOnlyList<StepResult>> Steps { get; }
```


Completed steps. Completes when the stream finishes.

**Value:** [Task&lt;IReadOnlyList&lt;StepResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2032)

<a id="Vercel_AI_StreamTextResult_Text_"></a>
<a id="Vercel_AI_StreamTextResult_Text"></a>

### Text


```csharp
public Task<string> Text { get; }
```


Full text from the last step. Completes when the stream finishes.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2023)

<a id="Vercel_AI_StreamTextResult_Usage_"></a>
<a id="Vercel_AI_StreamTextResult_Usage"></a>

### Usage


```csharp
public Task<LanguageModelUsage> Usage { get; }
```


Total usage. Completes when the stream finishes.

**Value:** [Task&lt;LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2029)

## Methods

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2017)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L2005)
