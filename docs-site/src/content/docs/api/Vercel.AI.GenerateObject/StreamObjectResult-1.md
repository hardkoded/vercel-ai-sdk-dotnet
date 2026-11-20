---
title: "StreamObjectResult<T>"
description: "The result of a streamObject call. Maps to StreamObjectResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamObjectResult<T>"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class StreamObjectResult<T>
```


The result of a `streamObject` call. Maps to `StreamObjectResult`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` | The result: the object, the array, or the enum value (a string). |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L257)

## Properties

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_FinishReason_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_FinishReason"></a>

### FinishReason


```csharp
public Task<FinishReason> FinishReason { get; }
```


The reason why the generation finished. Resolved when the response is finished.

**Value:** [Task&lt;FinishReason&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L295)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Object_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Object"></a>

### Object


```csharp
public Task<T> Object { get; }
```


The generated object. Resolved when the response is finished.

**Value:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L298)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_ProviderMetadata_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public Task<JsonElement?> ProviderMetadata { get; }
```


Additional provider-specific metadata.

**Value:** [Task&lt;JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L286)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Request_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Request"></a>

### Request


```csharp
public Task<LanguageModelRequestMetadata> Request { get; }
```


Additional request information.

**Value:** [Task&lt;LanguageModelRequestMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L289)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Response_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Response"></a>

### Response


```csharp
public Task<LanguageModelResponseMetadata> Response { get; }
```


Additional response information.

**Value:** [Task&lt;LanguageModelResponseMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L292)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Usage_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Usage"></a>

### Usage


```csharp
public Task<LanguageModelUsage> Usage { get; }
```


The token usage of the generated response. Resolved when the response is finished.

**Value:** [Task&lt;LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L283)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Warnings_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_Warnings"></a>

### Warnings


```csharp
public Task<IReadOnlyList<CallWarning>?> Warnings { get; }
```


Warnings from the model provider (e.g. unsupported settings).

**Value:** [Task&lt;IReadOnlyList&lt;CallWarning&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L280)

## Methods

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_ElementStream_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_ElementStream_System_Threading_CancellationToken_"></a>

### ElementStream(CancellationToken)


```csharp
public IAsyncEnumerable<JsonElement> ElementStream(CancellationToken cancellationToken = default)
```


Stream over complete array elements. Only available if the output is [Array](/api/Vercel.AI.GenerateObject/StreamObjectOutput/#Vercel_AI_GenerateObject_StreamObjectOutput_Array).
Errors are suppressed (use `OnError` instead).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | The output is not an array. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L322)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_FullStream_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_FullStream_System_Threading_CancellationToken_"></a>

### FullStream(CancellationToken)


```csharp
public IAsyncEnumerable<ObjectStreamPart> FullStream(CancellationToken cancellationToken = default)
```


Stream of different types of events, including partial objects, errors, and finish events. Only errors that stop the
stream, such as network errors, are thrown.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;ObjectStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L357)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_PartialObjectStream_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_PartialObjectStream_System_Threading_CancellationToken_"></a>

### PartialObjectStream(CancellationToken)


```csharp
public IAsyncEnumerable<JsonElement> PartialObjectStream(CancellationToken cancellationToken = default)
```


Stream of partial objects. It gets more complete as the stream progresses. The partial object is not validated.
Errors are suppressed (use `OnError` instead).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L304)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_PipeTextStreamToResponse_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_PipeTextStreamToResponse_Vercel_AI_Util_ServerResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_"></a>

### PipeTextStreamToResponse(ServerResponse, int?, string?, HeaderCollection?)


```csharp
public Task PipeTextStreamToResponse(ServerResponse response, int? status = null, string? statusText = null, HeaderCollection? headers = null)
```


Writes text delta output to a response. It sets a `content-type` header to `text/plain; charset=utf-8`
and writes each text delta as a separate chunk. Maps to `pipeTextStreamToResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L376)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_TextStream_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_TextStream_System_Threading_CancellationToken_"></a>

### TextStream(CancellationToken)


```csharp
public IAsyncEnumerable<string> TextStream(CancellationToken cancellationToken = default)
```


Text stream of the JSON representation of the generated object. It contains text chunks. When the stream is
finished, the object is valid JSON that can be parsed. Errors are suppressed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L340)

<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_ToTextStreamResponse_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectResult_1_ToTextStreamResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_"></a>

### ToTextStreamResponse(int?, string?, HeaderCollection?)


```csharp
public TextStreamResponse ToTextStreamResponse(int? status = null, string? statusText = null, HeaderCollection? headers = null)
```


Creates a simple text stream response. The response has a `content-type` header set to
`text/plain; charset=utf-8`. Each text delta is encoded as UTF-8 and sent as a separate chunk.
Maps to `toTextStreamResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |

**Returns:** [TextStreamResponse](/api/Vercel.AI.Util/TextStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L366)
