---
title: "ReasoningEndStreamPart"
description: "Marks the end of one reasoning block."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningEndStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ReasoningEndStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ReasoningEndStreamPart : LanguageModelStreamPart
```


Marks the end of one reasoning block.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2066)

## Constructors

<a id="Vercel_AI_Provider_ReasoningEndStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ReasoningEndStreamPart__ctor_System_String_"></a>

### ReasoningEndStreamPart(string)


```csharp
public ReasoningEndStreamPart(string id)
```


Creates a reasoning-end part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2069)

<a id="Vercel_AI_Provider_ReasoningEndStreamPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningEndStreamPart(string, JsonElement?)


```csharp
public ReasoningEndStreamPart(string id, JsonElement? providerMetadata)
```


Creates a reasoning-end part that carries provider metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2075)

## Properties

<a id="Vercel_AI_Provider_ReasoningEndStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ReasoningEndStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Reasoning block id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2083)

<a id="Vercel_AI_Provider_ReasoningEndStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ReasoningEndStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this block, such as encrypted reasoning content.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2086)
