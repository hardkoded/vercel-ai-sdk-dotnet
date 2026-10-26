---
title: "ToolInputEndStreamPart"
description: "Marks the end of streamed tool input."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputEndStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolInputEndStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolInputEndStreamPart : LanguageModelStreamPart
```


Marks the end of streamed tool input.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1834)

## Constructors

<a id="Vercel_AI_Provider_ToolInputEndStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ToolInputEndStreamPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolInputEndStreamPart(string, JsonElement?)


```csharp
public ToolInputEndStreamPart(string id, JsonElement? providerMetadata = null)
```


Creates a tool-input-end part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1837)

## Properties

<a id="Vercel_AI_Provider_ToolInputEndStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ToolInputEndStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1845)

<a id="Vercel_AI_Provider_ToolInputEndStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ToolInputEndStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1848)
