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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L150)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResult__ctor_Vercel_AI_StreamTextResult_System_Nullable_System_Int32__System_Func_System_Exception_System_String__System_Nullable_System_Boolean__System_Boolean_"></a>

### `UIMessageStreamResult(StreamTextResult, int?, Func<Exception, string>?, bool?, bool)`


```csharp
public UIMessageStreamResult(StreamTextResult result, int? keepAliveMs = null, Func<Exception, string>? onError = null, bool? redactToolErrors = null, bool sendSources = false)
```


Creates a result that writes &lt;code class="paramref"&gt;result&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. |
| `onError` | [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper from the error to the `error` chunk text. Null writes `An error occurred.`. It also formats failed tool calls when `redactToolErrors` is enabled. |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) | Replaces the error text of failed tool calls with the result of `onError`. Defaults to `true` when `onError` is provided and to `false` otherwise. |
| `sendSources` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | Whether source parts are sent to the client. Defaults to `false`. Maps to `sendSources`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L180)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L197)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_WriteAsync_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResult_WriteAsync_Vercel_AI_StreamTextResult_System_IO_Stream_System_Nullable_System_Int32__System_Threading_CancellationToken_System_Func_System_Exception_System_String__System_Nullable_System_Boolean__System_Boolean_"></a>

### `WriteAsync(StreamTextResult, Stream, int?, CancellationToken, Func<Exception, string>?, bool?, bool)`


```csharp
public static Task WriteAsync(StreamTextResult result, Stream destination, int? keepAliveMs = null, CancellationToken cancellationToken = default, Func<Exception, string>? onError = null, bool? redactToolErrors = null, bool sendSources = false)
```


Writes the UI message stream to &lt;code class="paramref"&gt;destination&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `destination` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) | The response body or another writable stream. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. A positive value up to 2147483647 writes `: stream-open` before the start event and `: keep-alive` while the next part is still pending. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the write and stops keep-alive comments. ASP.NET Core passes `HttpContext.RequestAborted`. |
| `onError` | [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper from the error to the `error` chunk text. Null writes `An error occurred.`. It also formats failed tool calls when `redactToolErrors` is enabled. |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) | Replaces the error text of failed tool calls with the result of `onError`. Defaults to `true` when `onError` is provided and to `false` otherwise. |
| `sendSources` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | Whether source parts are sent to the client. Defaults to `false`. Maps to `sendSources`. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L228)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L354)
