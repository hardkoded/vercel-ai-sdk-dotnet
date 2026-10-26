---
title: "TextStartStreamPart"
description: "Marks the start of one text block."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextStartStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_TextStartStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class TextStartStreamPart : LanguageModelStreamPart
```


Marks the start of one text block.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2008)

## Constructors

<a id="Vercel_AI_Provider_TextStartStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_TextStartStreamPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### TextStartStreamPart(string, JsonElement?)


```csharp
public TextStartStreamPart(string id, JsonElement? providerMetadata = null)
```


Creates a text-start part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2011)

## Properties

<a id="Vercel_AI_Provider_TextStartStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_TextStartStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Text block id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2019)

<a id="Vercel_AI_Provider_TextStartStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_TextStartStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2022)
