---
title: "ReasoningDeltaStreamPart"
description: "A reasoning delta."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningDeltaStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ReasoningDeltaStreamPart : LanguageModelStreamPart
```


A reasoning delta.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1682)

## Constructors

<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningDeltaStreamPart(string, string, JsonElement?)


```csharp
public ReasoningDeltaStreamPart(string id, string delta, JsonElement? providerMetadata = null)
```


Creates a reasoning delta.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `delta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1685)

## Properties

<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart_Delta_"></a>
<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart_Delta"></a>

### Delta


```csharp
public string Delta { get; }
```


Reasoning fragment.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1697)

<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Reasoning block id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1694)

<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ReasoningDeltaStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this fragment, such as a reasoning signature.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1700)
