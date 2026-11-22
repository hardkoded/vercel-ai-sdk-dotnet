---
title: "StreamTextChunkEvent"
description: "Passed to OnChunk. Maps to the event of onChunk."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextChunkEvent"
  hidden: true
---

<a id="Vercel_AI_StreamTextChunkEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextChunkEvent
```


Passed to [OnChunk](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnChunk). Maps to the event of `onChunk`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2188)

## Constructors

<a id="Vercel_AI_StreamTextChunkEvent__ctor_"></a>
<a id="Vercel_AI_StreamTextChunkEvent__ctor_Vercel_AI_TextStreamPart_"></a>

### StreamTextChunkEvent(TextStreamPart)


```csharp
public StreamTextChunkEvent(TextStreamPart chunk)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | [TextStreamPart](/api/Vercel.AI/TextStreamPart/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2191)

## Properties

<a id="Vercel_AI_StreamTextChunkEvent_Chunk_"></a>
<a id="Vercel_AI_StreamTextChunkEvent_Chunk"></a>

### Chunk


```csharp
public TextStreamPart Chunk { get; }
```


The stream part.

**Value:** [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2197)
