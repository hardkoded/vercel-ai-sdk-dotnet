---
title: "GeneratedReasoning"
description: "Reasoning text kept separate from the answer."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedReasoning"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedReasoning"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class GeneratedReasoning : GeneratedContent
```


Reasoning text kept separate from the answer.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

**Derived:** [GoogleReasoning](/api/Vercel.AI.Google/GoogleReasoning/), [HuggingFaceReasoning](/api/Vercel.AI.HuggingFace/HuggingFaceReasoning/), [XaiReasoning](/api/Vercel.AI.Xai/XaiReasoning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L278)

## Constructors

<a id="Vercel_AI_Provider_GeneratedReasoning__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoning__ctor_System_String_"></a>

### GeneratedReasoning(string)


```csharp
public GeneratedReasoning(string text)
```


Creates reasoning content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L281)

<a id="Vercel_AI_Provider_GeneratedReasoning__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedReasoning(string, JsonElement?)


```csharp
public GeneratedReasoning(string text, JsonElement? providerMetadata)
```


Creates reasoning content that carries provider metadata, such as an `itemId` or encrypted content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L287)

## Properties

<a id="Vercel_AI_Provider_GeneratedReasoning_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoning_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this reasoning, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L298)

<a id="Vercel_AI_Provider_GeneratedReasoning_Text_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoning_Text"></a>

### Text


```csharp
public string Text { get; }
```


Reasoning text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L295)
