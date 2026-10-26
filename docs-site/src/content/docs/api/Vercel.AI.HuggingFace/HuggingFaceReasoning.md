---
title: "HuggingFaceReasoning"
description: "Reasoning text that carries the Responses output item id."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceReasoning"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceReasoning"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public sealed class HuggingFaceReasoning : GeneratedReasoning
```


Reasoning text that carries the Responses output item id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedReasoning](/api/Vercel.AI.Provider/GeneratedReasoning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L25)

## Constructors

<a id="Vercel_AI_HuggingFace_HuggingFaceReasoning__ctor_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceReasoning__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### HuggingFaceReasoning(string, JsonElement?)


```csharp
public HuggingFaceReasoning(string text, JsonElement? providerMetadata)
```


Creates reasoning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L28)
