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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5065)

## Properties

<a id="Vercel_AI_StreamTextSingleConsumerResult_FinishReason_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_FinishReason"></a>

### FinishReason


```csharp
public Task<FinishReason> FinishReason { get; }
```


Finish reason. Completes when the stream finishes. Maps to `finishReason`.

**Value:** [Task&lt;FinishReason&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5078)

<a id="Vercel_AI_StreamTextSingleConsumerResult_RawFinishReason_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_RawFinishReason"></a>

### RawFinishReason


```csharp
public Task<string?> RawFinishReason { get; }
```


The finish reason as the provider reported it. Completes when the stream finishes. Maps to `rawFinishReason`.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5081)

<a id="Vercel_AI_StreamTextSingleConsumerResult_Usage_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_Usage"></a>

### Usage


```csharp
public Task<LanguageModelUsage> Usage { get; }
```


Total usage. Completes when the stream finishes. Maps to `totalUsage` and `usage`.

**Value:** [Task&lt;LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5075)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5084)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5192)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5184)

<a id="Vercel_AI_StreamTextSingleConsumerResult_PipeTextStreamToResponseAsync_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_PipeTextStreamToResponseAsync_Vercel_AI_Util_ServerResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_"></a>

### PipeTextStreamToResponseAsync(ServerResponse, int?, string?, HeaderCollection?, CancellationToken)


```csharp
public Task PipeTextStreamToResponseAsync(ServerResponse response, int? status = null, string? statusText = null, HeaderCollection? headers = null, CancellationToken cancellationToken = default)
```


Pipes the text deltas to &lt;code class="paramref"&gt;response&lt;/code&gt;. Maps to `pipeTextStreamToResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5151)

<a id="Vercel_AI_StreamTextSingleConsumerResult_PipeUIMessageStreamToResponseAsync_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_PipeUIMessageStreamToResponseAsync_Vercel_AI_Util_ServerResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Nullable_System_Int32__System_Func_System_Exception_System_String__System_Nullable_System_Boolean__System_Threading_CancellationToken_System_Boolean_"></a>

### `PipeUIMessageStreamToResponseAsync(ServerResponse, int?, string?, HeaderCollection?, int?, Func<Exception, string>?, bool?, CancellationToken, bool)`


```csharp
public Task PipeUIMessageStreamToResponseAsync(ServerResponse response, int? status = null, string? statusText = null, HeaderCollection? headers = null, int? keepAliveMs = null, Func<Exception, string>? onError = null, bool? redactToolErrors = null, CancellationToken cancellationToken = default, bool sendSources = false)
```


Pipes the UI message stream to &lt;code class="paramref"&gt;response&lt;/code&gt;. Maps to `pipeUIMessageStreamToResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `onError` | [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `sendSources` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5124)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5175)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5169)

<a id="Vercel_AI_StreamTextSingleConsumerResult_ToTextStreamResponse_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_ToTextStreamResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_"></a>

### ToTextStreamResponse(int?, string?, HeaderCollection?, CancellationToken)


```csharp
public TextStreamResponse ToTextStreamResponse(int? status = null, string? statusText = null, HeaderCollection? headers = null, CancellationToken cancellationToken = default)
```


Builds a `text/plain` response of the text deltas. Maps to `toTextStreamResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [TextStreamResponse](/api/Vercel.AI.Util/TextStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5112)

<a id="Vercel_AI_StreamTextSingleConsumerResult_ToUIMessageStream_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_ToUIMessageStream_Vercel_AI_AspNetCore_UIMessageStreamOptions_System_Threading_CancellationToken_"></a>

### ToUIMessageStream(UIMessageStreamOptions?, CancellationToken)


```csharp
public IAsyncEnumerable<JsonObject> ToUIMessageStream(UIMessageStreamOptions? options = null, CancellationToken cancellationToken = default)
```


Converts the stream parts into UI message chunks. Maps to `toUIMessageStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [UIMessageStreamOptions](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5091)

<a id="Vercel_AI_StreamTextSingleConsumerResult_ToUIMessageStreamResult_"></a>
<a id="Vercel_AI_StreamTextSingleConsumerResult_ToUIMessageStreamResult_System_Nullable_System_Int32__System_Func_System_Exception_System_String__System_Nullable_System_Boolean__System_Boolean_"></a>

### `ToUIMessageStreamResult(int?, Func<Exception, string>?, bool?, bool)`


```csharp
public IResult ToUIMessageStreamResult(int? keepAliveMs = null, Func<Exception, string>? onError = null, bool? redactToolErrors = null, bool sendSources = false)
```


Returns a `text/event-stream` result of the UI message stream. Maps to `toUIMessageStreamResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `onError` | [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `sendSources` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [IResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Generation.cs#L5100)
