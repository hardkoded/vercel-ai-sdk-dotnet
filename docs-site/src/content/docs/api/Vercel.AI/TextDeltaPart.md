---
title: "TextDeltaPart"
description: "Text delta. Maps to text-delta."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextDeltaPart"
  hidden: true
---

<a id="Vercel_AI_TextDeltaPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TextDeltaPart : TextStreamPart
```


Text delta. Maps to `text-delta`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1948)

## Constructors

<a id="Vercel_AI_TextDeltaPart__ctor_"></a>
<a id="Vercel_AI_TextDeltaPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### TextDeltaPart(string, string, JsonElement?)


```csharp
public TextDeltaPart(string id, string text, JsonElement? providerMetadata = null)
```


Creates a text delta.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1951)

## Properties

<a id="Vercel_AI_TextDeltaPart_Id_"></a>
<a id="Vercel_AI_TextDeltaPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Id of the text block.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1960)

<a id="Vercel_AI_TextDeltaPart_ProviderMetadata_"></a>
<a id="Vercel_AI_TextDeltaPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the delta.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1966)

<a id="Vercel_AI_TextDeltaPart_Text_"></a>
<a id="Vercel_AI_TextDeltaPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Delta text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1963)
