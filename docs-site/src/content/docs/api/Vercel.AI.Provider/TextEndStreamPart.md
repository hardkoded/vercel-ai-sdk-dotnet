---
title: "TextEndStreamPart"
description: "Marks the end of one text block."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextEndStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_TextEndStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class TextEndStreamPart : LanguageModelStreamPart
```


Marks the end of one text block.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2026)

## Constructors

<a id="Vercel_AI_Provider_TextEndStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_TextEndStreamPart__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_Citation__System_Nullable_System_Text_Json_JsonElement__"></a>

### `TextEndStreamPart(string, IReadOnlyList<Citation>?, JsonElement?)`


```csharp
public TextEndStreamPart(string id, IReadOnlyList<Citation>? citations = null, JsonElement? providerMetadata = null)
```


Creates a text-end part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `citations` | [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2029)

## Properties

<a id="Vercel_AI_Provider_TextEndStreamPart_Citations_"></a>
<a id="Vercel_AI_Provider_TextEndStreamPart_Citations"></a>

### Citations


```csharp
public IReadOnlyList<Citation>? Citations { get; }
```


References supporting the text block with this id, or null when there are none.

**Value:** [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2041)

<a id="Vercel_AI_Provider_TextEndStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_TextEndStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Text block id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2038)

<a id="Vercel_AI_Provider_TextEndStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_TextEndStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2044)
