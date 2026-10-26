---
title: "RawPart"
description: "A raw chunk of the provider stream. It appears only when RawChunks is set."
editUrl: false
prev: false
next: false
sidebar:
  label: "RawPart"
  hidden: true
---

<a id="Vercel_AI_RawPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class RawPart : TextStreamPart
```


A raw chunk of the provider stream. It appears only when [RawChunks](/api/Vercel.AI/StreamTextInclude/#Vercel_AI_StreamTextInclude_RawChunks) is set.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1133)

## Constructors

<a id="Vercel_AI_RawPart__ctor_"></a>
<a id="Vercel_AI_RawPart__ctor_System_Text_Json_JsonElement_"></a>

### RawPart(JsonElement)


```csharp
public RawPart(JsonElement rawValue)
```


Creates a raw part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `rawValue` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1136)

## Properties

<a id="Vercel_AI_RawPart_RawValue_"></a>
<a id="Vercel_AI_RawPart_RawValue"></a>

### RawValue


```csharp
public JsonElement RawValue { get; }
```


The raw value the provider sent. Maps to `rawValue`.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1143)
