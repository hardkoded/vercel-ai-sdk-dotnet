---
title: "StreamTextSingleConsumerResult"
description: "A single-consumer view of a text generation, without replay or final content members. Maps to StreamTextSingleConsumerResult. Choose one stream projection. A..."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextSingleConsumerResult"
  hidden: true
---

<a id="Vercel_AI_StreamTextSingleConsumerResult"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextSingleConsumerResult
```


A single-consumer view of a text generation, without replay or final content members. Maps to `StreamTextSingleConsumerResult`.
Choose one stream projection. A second one throws.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4441)

## Properties

<a id="Vercel_AI_StreamTextSingleConsumerResult_FinishReason_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_FinishReason"></a>

### FinishReason


```csharp
public Task<FinishReason> FinishReason { get; }
```


Finish reason. Completes when the stream finishes. Maps to `finishReason`.

**Value:** [Task&lt;FinishReason&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4454)

<a id="Vercel_AI_StreamTextSingleConsumerResult_Usage_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_Usage"></a>

### Usage


```csharp
public Task<LanguageModelUsage> Usage { get; }
```


Total usage. Completes when the stream finishes. Maps to `totalUsage` and `usage`.

**Value:** [Task&lt;LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4451)

## Methods

<a id="Vercel_AI_StreamTextSingleConsumerResult_ConsumeStreamAsync_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_ConsumeStreamAsync_System_Threading_CancellationToken_"></a>

### ConsumeStreamAsync(CancellationToken)


```csharp
public Task ConsumeStreamAsync(CancellationToken cancellationToken = default)
```


Consumes the stream without processing the parts. Maps to `consumeStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4457)

<a id="Vercel_AI_StreamTextSingleConsumerResult_ElementStream_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_ElementStream_System_Threading_CancellationToken_"></a>

### ElementStream(CancellationToken)


```csharp
public IAsyncEnumerable<JsonElement> ElementStream(CancellationToken cancellationToken = default)
```


The elements of an array output as each one completes. Maps to `elementStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4486)

<a id="Vercel_AI_StreamTextSingleConsumerResult_PartialOutputStream_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_PartialOutputStream_System_Threading_CancellationToken_"></a>

### PartialOutputStream(CancellationToken)


```csharp
public IAsyncEnumerable<JsonElement> PartialOutputStream(CancellationToken cancellationToken = default)
```


The partial structured output. It requires an explicit output specification, otherwise it throws
[UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/). Maps to `partialOutputStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4478)

<a id="Vercel_AI_StreamTextSingleConsumerResult_Stream_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_Stream_System_Threading_CancellationToken_"></a>

### Stream(CancellationToken)


```csharp
public IAsyncEnumerable<TextStreamPart> Stream(CancellationToken cancellationToken = default)
```


Every stream part. Maps to `stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;TextStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4469)

<a id="Vercel_AI_StreamTextSingleConsumerResult_TextStream_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_TextStream_System_Threading_CancellationToken_"></a>

### TextStream(CancellationToken)


```csharp
public IAsyncEnumerable<string> TextStream(CancellationToken cancellationToken = default)
```


Text deltas only. Maps to `textStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L4463)
