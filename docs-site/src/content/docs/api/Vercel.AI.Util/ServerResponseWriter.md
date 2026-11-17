---
title: "ServerResponseWriter"
description: "Writes a byte stream to a ServerResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "ServerResponseWriter"
  hidden: true
---

<a id="Vercel_AI_Util_ServerResponseWriter"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class ServerResponseWriter
```


Writes a byte stream to a [ServerResponse](/api/Vercel.AI.Util/ServerResponse/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L223)

## Methods

<a id="Vercel_AI_Util_ServerResponseWriter_WriteAsync_"></a>
<a id="Vercel_AI_Util_ServerResponseWriter_WriteAsync_Vercel_AI_Util_ServerResponse_Vercel_AI_Util_ReadableStream_System_Byte____System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_"></a>

### `WriteAsync(ServerResponse?, ReadableStream<byte[]>, int?, string?, HeaderCollection?, CancellationToken)`


```csharp
public static Task WriteAsync(ServerResponse? response, ReadableStream<byte[]> stream, int? status = null, string? statusText = null, HeaderCollection? headers = null, CancellationToken cancellationToken = default)
```


Sets status and headers, then writes &lt;code class="paramref"&gt;stream&lt;/code&gt;.
A false write waits for [SimulateDrain()](/api/Vercel.AI.Util/ServerResponse/#Vercel_AI_Util_ServerResponse_SimulateDrain).
The response is ended even when reading the stream fails.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) |  |
| `stream` | `ReadableStream&lt;byte\[\]&gt;` |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L230)
