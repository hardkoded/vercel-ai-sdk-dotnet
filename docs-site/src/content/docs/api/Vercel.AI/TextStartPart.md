---
title: "TextStartPart"
description: "A text block started. Maps to text-start."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextStartPart"
  hidden: true
---

<a id="Vercel_AI_TextStartPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TextStartPart : TextStreamPart
```


A text block started. Maps to `text-start`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2653)

## Constructors

<a id="Vercel_AI_TextStartPart__ctor_"></a>
<a id="Vercel_AI_TextStartPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### TextStartPart(string, JsonElement?)


```csharp
public TextStartPart(string id, JsonElement? providerMetadata = null)
```


Creates a text start.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2656)

## Properties

<a id="Vercel_AI_TextStartPart_Id_"></a>
<a id="Vercel_AI_TextStartPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Id of the text block.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2664)

<a id="Vercel_AI_TextStartPart_ProviderMetadata_"></a>
<a id="Vercel_AI_TextStartPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the text block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2667)
