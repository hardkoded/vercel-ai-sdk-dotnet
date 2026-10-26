---
title: "XaiReasoning"
description: "Reasoning text that carries the xAI Responses item id and encrypted content."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiReasoning"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiReasoning"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiReasoning : GeneratedReasoning
```


Reasoning text that carries the xAI Responses item id and encrypted content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedReasoning](/api/Vercel.AI.Provider/GeneratedReasoning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L16)

## Constructors

<a id="Vercel_AI_Xai_XaiReasoning__ctor_"></a>
<a id="Vercel_AI_Xai_XaiReasoning__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### XaiReasoning(string, JsonElement?)


```csharp
public XaiReasoning(string text, JsonElement? providerMetadata)
```


Creates reasoning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L19)
