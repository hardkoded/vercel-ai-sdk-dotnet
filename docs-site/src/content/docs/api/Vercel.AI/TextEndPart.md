---
title: "TextEndPart"
description: "A text block ended. Maps to text-end."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextEndPart"
  hidden: true
---

<a id="Vercel_AI_TextEndPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TextEndPart : TextStreamPart
```


A text block ended. Maps to `text-end`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1831)

## Constructors

<a id="Vercel_AI_TextEndPart__ctor_"></a>
<a id="Vercel_AI_TextEndPart__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_Citation__System_Nullable_System_Text_Json_JsonElement__"></a>

### `TextEndPart(string, IReadOnlyList<Citation>?, JsonElement?)`


```csharp
public TextEndPart(string id, IReadOnlyList<Citation>? citations = null, JsonElement? providerMetadata = null)
```


Creates a text end.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `citations` | [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1834)

## Properties

<a id="Vercel_AI_TextEndPart_Citations_"></a>
<a id="Vercel_AI_TextEndPart_Citations"></a>

### Citations


```csharp
public IReadOnlyList<Citation>? Citations { get; }
```


References that support the text block, or null when there are none.

**Value:** [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1850)

<a id="Vercel_AI_TextEndPart_Id_"></a>
<a id="Vercel_AI_TextEndPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Id of the text block.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1847)

<a id="Vercel_AI_TextEndPart_ProviderMetadata_"></a>
<a id="Vercel_AI_TextEndPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the text block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1853)
