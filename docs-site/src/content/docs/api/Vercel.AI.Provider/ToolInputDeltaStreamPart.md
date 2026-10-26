---
title: "ToolInputDeltaStreamPart"
description: "A streamed tool-argument fragment."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputDeltaStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolInputDeltaStreamPart : LanguageModelStreamPart
```


A streamed tool-argument fragment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1812)

## Constructors

<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolInputDeltaStreamPart(string, string, JsonElement?)


```csharp
public ToolInputDeltaStreamPart(string id, string delta, JsonElement? providerMetadata = null)
```


Creates a tool-input-delta part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `delta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1815)

## Properties

<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart_Delta_"></a>
<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart_Delta"></a>

### Delta


```csharp
public string Delta { get; }
```


JSON fragment.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1827)

<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1824)

<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ToolInputDeltaStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this fragment.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1830)
