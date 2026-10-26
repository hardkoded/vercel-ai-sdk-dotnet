---
title: "TextStreams"
description: "Text-stream helpers: keep text deltas, build a response, and write it to a server response."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextStreams"
  hidden: true
---

<a id="Vercel_AI_Util_TextStreams"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class TextStreams
```


Text-stream helpers: keep text deltas, build a response, and write it to a server response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L60)

## Methods

<a id="Vercel_AI_Util_TextStreams_CreateTextStreamResponse_"></a>
<a id="Vercel_AI_Util_TextStreams_CreateTextStreamResponse_Vercel_AI_Util_ReadableStream_System_String__System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_"></a>

### `CreateTextStreamResponse(ReadableStream<string>, int?, string?, HeaderCollection?)`


```csharp
public static TextStreamResponse CreateTextStreamResponse(ReadableStream<string> stream, int? status = null, string? statusText = null, HeaderCollection? headers = null)
```


Builds a response with `content-type: text/plain; charset=utf-8` unless already set.
Each string chunk is encoded as its own UTF-8 body chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | `ReadableStream&lt;string&gt;` |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |

**Returns:** [TextStreamResponse](/api/Vercel.AI.Util/TextStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L96)

<a id="Vercel_AI_Util_TextStreams_PipeTextStreamToResponseAsync_"></a>
<a id="Vercel_AI_Util_TextStreams_PipeTextStreamToResponseAsync_Vercel_AI_Util_ServerResponse_Vercel_AI_Util_ReadableStream_System_String__System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_"></a>

### `PipeTextStreamToResponseAsync(ServerResponse, ReadableStream<string>, int?, string?, HeaderCollection?)`


```csharp
public static Task PipeTextStreamToResponseAsync(ServerResponse response, ReadableStream<string> stream, int? status = null, string? statusText = null, HeaderCollection? headers = null)
```


Encodes &lt;code class="paramref"&gt;stream&lt;/code&gt; as UTF-8 and writes it to &lt;code class="paramref"&gt;response&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) |  |
| `stream` | `ReadableStream&lt;string&gt;` |  |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L106)

<a id="Vercel_AI_Util_TextStreams_ToTextStream_"></a>
<a id="Vercel_AI_Util_TextStreams_ToTextStream_Vercel_AI_Util_ReadableStream_Vercel_AI_Util_TextStreamPart__"></a>

### `ToTextStream(ReadableStream<TextStreamPart>)`


```csharp
public static ReadableStream<string> ToTextStream(ReadableStream<TextStreamPart> stream)
```


Yields only `text-delta` text from &lt;code class="paramref"&gt;stream&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | `ReadableStream&lt;TextStreamPart&gt;` |  |

**Returns:** `ReadableStream&lt;string&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L63)
