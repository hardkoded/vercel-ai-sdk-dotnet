---
title: "ReasoningStartStreamPart"
description: "Marks the start of one reasoning block."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningStartStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ReasoningStartStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ReasoningStartStreamPart : LanguageModelStreamPart
```


Marks the start of one reasoning block.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2048)

## Constructors

<a id="Vercel_AI_Provider_ReasoningStartStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ReasoningStartStreamPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningStartStreamPart(string, JsonElement?)


```csharp
public ReasoningStartStreamPart(string id, JsonElement? providerMetadata = null)
```


Creates a reasoning-start part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2051)

## Properties

<a id="Vercel_AI_Provider_ReasoningStartStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ReasoningStartStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Reasoning block id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2059)

<a id="Vercel_AI_Provider_ReasoningStartStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ReasoningStartStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2062)
