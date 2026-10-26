---
title: "GoogleThinking"
description: "Maps the shared reasoning option onto Gemini thinking configuration."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleThinking"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleThinking"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleThinking
```


Maps the shared reasoning option onto Gemini thinking configuration.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleThinking.cs#L12)

## Methods

<a id="Vercel_AI_Google_GoogleThinking_Resolve_"></a>
<a id="Vercel_AI_Google_GoogleThinking_Resolve_System_String_System_String_System_Collections_Generic_IList_Vercel_AI_Google_GoogleWarning__"></a>

### `Resolve(string?, string, IList<GoogleWarning>)`


```csharp
public static JsonObject? Resolve(string? reasoning, string modelId, IList<GoogleWarning> warnings)
```


Returns a `thinkingConfig` object, or null when reasoning is unset or `provider-default`.
Provider options are applied later and override these fields.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleThinking.cs#L22)
