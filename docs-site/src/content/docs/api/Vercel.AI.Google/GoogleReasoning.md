---
title: "GoogleReasoning"
description: "Reasoning text that can carry a thought signature."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleReasoning"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleReasoning"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleReasoning : GeneratedReasoning
```


Reasoning text that can carry a thought signature.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedReasoning](/api/Vercel.AI.Provider/GeneratedReasoning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L455)

## Constructors

<a id="Vercel_AI_Google_GoogleReasoning__ctor_"></a>
<a id="Vercel_AI_Google_GoogleReasoning__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GoogleReasoning(string, JsonElement?)


```csharp
public GoogleReasoning(string text, JsonElement? providerMetadata)
```


Creates reasoning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L458)
