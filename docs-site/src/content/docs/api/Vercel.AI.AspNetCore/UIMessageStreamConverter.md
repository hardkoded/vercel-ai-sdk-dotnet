---
title: "UIMessageStreamConverter"
description: "Converts streamText parts into AI SDK UI message chunks."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamConverter"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public static class UIMessageStreamConverter
```


Converts `streamText` parts into AI SDK UI message chunks.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L15)

## Methods

<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter_ReadUIMessageStream_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter_ReadUIMessageStream_Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_System_Threading_CancellationToken_"></a>

### ReadUIMessageStream(ReadUIMessageStreamOptions, CancellationToken)


```csharp
public static IAsyncEnumerable<UIMessage> ReadUIMessageStream(ReadUIMessageStreamOptions options, CancellationToken cancellationToken = default)
```


Transforms a stream of UI message chunks into a stream of [UIMessage](/api/Vercel.AI.Ui/UIMessage/)s.
Each element is a different state of the same message as it is being completed. Maps to `readUIMessageStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ReadUIMessageStreamOptions](/api/Vercel.AI.AspNetCore/ReadUIMessageStreamOptions/) | The stream to read and how to handle errors. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the read of the stream. |

**Returns:** [IAsyncEnumerable&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L41)

<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter_ToUIMessageChunk_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter_ToUIMessageChunk_Vercel_AI_TextStreamPart_Vercel_AI_AspNetCore_ToUIMessageChunkOptions_"></a>

### ToUIMessageChunk(TextStreamPart, ToUIMessageChunkOptions?)


```csharp
public static JsonObject? ToUIMessageChunk(TextStreamPart part, ToUIMessageChunkOptions? options = null)
```


Converts one [TextStreamPart](/api/Vercel.AI/TextStreamPart/) into a UI message chunk.
Returns &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for stream parts that do not produce UI message chunks.
The .NET stream has no text-start, text-end, reasoning-start, or reasoning-end parts, so `CancellationToken)` adds those chunks.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `part` | [TextStreamPart](/api/Vercel.AI/TextStreamPart/) | The part to convert. |
| `options` | [ToUIMessageChunkOptions](/api/Vercel.AI.AspNetCore/ToUIMessageChunkOptions/) | Conversion options. |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L67)

<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter_ToUIMessageStream_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamConverter_ToUIMessageStream_System_Collections_Generic_IAsyncEnumerable_Vercel_AI_TextStreamPart__Vercel_AI_AspNetCore_ToUIMessageChunkOptions_System_Threading_CancellationToken_"></a>

### `ToUIMessageStream(IAsyncEnumerable<TextStreamPart>, ToUIMessageChunkOptions?, CancellationToken)`


```csharp
public static IAsyncEnumerable<JsonObject> ToUIMessageStream(IAsyncEnumerable<TextStreamPart> stream, ToUIMessageChunkOptions? options = null, CancellationToken cancellationToken = default)
```


Converts a stream of [TextStreamPart](/api/Vercel.AI/TextStreamPart/) parts (as emitted by `streamText`) into a stream of UI message chunks.
It adds the text and reasoning start and end chunks, which the .NET stream parts do not carry, and the response message id.
It also calls [OnStepEnd](/api/Vercel.AI.AspNetCore/ToUIMessageChunkOptions/#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnStepEnd) and [OnEnd](/api/Vercel.AI.AspNetCore/ToUIMessageChunkOptions/#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnEnd).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [IAsyncEnumerable&lt;TextStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) | The parts to convert. |
| `options` | [ToUIMessageChunkOptions](/api/Vercel.AI.AspNetCore/ToUIMessageChunkOptions/) | Conversion options. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the read of &lt;code class="paramref"&gt;stream&lt;/code&gt;. |

**Returns:** [IAsyncEnumerable&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L25)
