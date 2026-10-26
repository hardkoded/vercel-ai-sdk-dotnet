---
title: "UIMessageStreamResult"
description: "SSE result for the AI SDK UI message protocol."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamResult"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamResult : IResult
```


SSE result for the AI SDK UI message protocol.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L48)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResult__ctor_Vercel_AI_StreamTextResult_System_Nullable_System_Int32__System_Func_System_String_System_String__"></a>

### `UIMessageStreamResult(StreamTextResult, int?, Func<string, string>?)`


```csharp
public UIMessageStreamResult(StreamTextResult result, int? keepAliveMs = null, Func<string, string>? onError = null)
```


Creates a result that writes &lt;code class="paramref"&gt;result&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. |
| `onError` | [Func&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper for `error` chunk text. Null writes `An error occurred.`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L62)

## Methods

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_ExecuteAsync_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_ExecuteAsync_Microsoft_AspNetCore_Http_HttpContext_"></a>

### ExecuteAsync(HttpContext)


```csharp
public Task ExecuteAsync(HttpContext httpContext)
```


Write an HTTP response reflecting the result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpContext` | [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext) | The [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext) for the current request. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task) — A task that represents the asynchronous execute operation.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L71)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_WriteAsync_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_WriteAsync_Vercel_AI_StreamTextResult_System_IO_Stream_System_Nullable_System_Int32__System_Threading_CancellationToken_System_Func_System_String_System_String__"></a>

### `WriteAsync(StreamTextResult, Stream, int?, CancellationToken, Func<string, string>?)`


```csharp
public static Task WriteAsync(StreamTextResult result, Stream destination, int? keepAliveMs = null, CancellationToken cancellationToken = default, Func<string, string>? onError = null)
```


Writes the UI message stream to &lt;code class="paramref"&gt;destination&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `destination` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) | The response body or another writable stream. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. A positive value up to 2147483647 writes `: stream-open` before the start event and `: keep-alive` while the next part is still pending. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the write and stops keep-alive comments. ASP.NET Core passes `HttpContext.RequestAborted`. |
| `onError` | [Func&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper for `error` chunk text. Null writes `An error occurred.`. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L91)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_WriteChunksAsync_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_WriteChunksAsync_System_Collections_Generic_IReadOnlyList_System_Text_Json_Nodes_JsonObject__System_IO_Stream_System_Threading_CancellationToken_"></a>

### `WriteChunksAsync(IReadOnlyList<JsonObject>, Stream, CancellationToken)`


```csharp
public static Task WriteChunksAsync(IReadOnlyList<JsonObject> chunks, Stream destination, CancellationToken cancellationToken = default)
```


Writes already-built UI message chunks as SSE, then `data: [DONE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunks` | [IReadOnlyList&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Chunk objects. Each one is one `data:` frame. |
| `destination` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) | The response body or another writable stream. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the write. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L166)
