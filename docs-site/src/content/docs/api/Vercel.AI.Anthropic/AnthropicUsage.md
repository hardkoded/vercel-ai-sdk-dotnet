---
title: "AnthropicUsage"
description: "Converts an Anthropic usage object into SDK token counts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicUsage"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicUsage"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicUsage
```


Converts an Anthropic `usage` object into SDK token counts.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicUsage.cs#L11)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicUsage_Convert_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUsage_Convert_System_Text_Json_JsonElement_System_Nullable_System_Text_Json_JsonElement__"></a>

### Convert(JsonElement, JsonElement?)


```csharp
public static LanguageModelUsage Convert(JsonElement usage, JsonElement? rawUsage = null)
```


Maps Anthropic usage. Cache creation and cache reads are added to the input total.
Compaction and message iterations are summed when present. Advisor iterations are left out
because they bill at a different rate. A `fallback_message` iteration keeps the top-level totals.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `rawUsage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicUsage.cs#L18)
