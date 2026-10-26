---
title: "HuggingFaceText"
description: "Generated text that carries the Responses output item id."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceText"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceText"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public sealed class HuggingFaceText : GeneratedText
```


Generated text that carries the Responses output item id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedText](/api/Vercel.AI.Provider/GeneratedText/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L11)

## Constructors

<a id="Vercel_AI_HuggingFace_HuggingFaceText__ctor_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceText__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### HuggingFaceText(string, JsonElement?)


```csharp
public HuggingFaceText(string text, JsonElement? providerMetadata)
```


Creates text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L14)

## Properties

<a id="Vercel_AI_HuggingFace_HuggingFaceText_ProviderMetadata_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceText_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata, such as `huggingface.itemId`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L21)
