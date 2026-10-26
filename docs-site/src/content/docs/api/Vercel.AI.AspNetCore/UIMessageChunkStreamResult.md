---
title: "UIMessageChunkStreamResult"
description: "SSE response for UI message chunks that are already built."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageChunkStreamResult"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageChunkStreamResult"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageChunkStreamResult : IResult
```


SSE response for UI message chunks that are already built.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunkStream.cs#L11)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageChunkStreamResult__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageChunkStreamResult__ctor_System_Collections_Generic_IReadOnlyList_System_Text_Json_Nodes_JsonObject__System_Int32_System_Collections_Generic_IReadOnlyList_System_Collections_Generic_KeyValuePair_System_String_System_String___"></a>

### `UIMessageChunkStreamResult(IReadOnlyList<JsonObject>, int, IReadOnlyList<KeyValuePair<string, string>>?)`


```csharp
public UIMessageChunkStreamResult(IReadOnlyList<JsonObject> chunks, int statusCode = 200, IReadOnlyList<KeyValuePair<string, string>>? headers = null)
```


Creates a response that frames &lt;code class="paramref"&gt;chunks&lt;/code&gt; and then writes `data: [DONE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunks` | [IReadOnlyList&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Chunk objects, in order. |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) | HTTP status code. Defaults to 200. |
| `headers` | [IReadOnlyList&lt;KeyValuePair&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Extra headers. Repeated names, including `Set-Cookie`, are all written. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunkStream.cs#L21)

## Methods

<a id="Vercel_AI_AspNetCore_UIMessageChunkStreamResult_ExecuteAsync_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageChunkStreamResult_ExecuteAsync_Microsoft_AspNetCore_Http_HttpContext_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunkStream.cs#L37)
