---
title: "AnthropicReasoning"
description: "Maps SDK reasoning levels onto Anthropic thinking and effort."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicReasoning"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicReasoning"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicReasoning
```


Maps SDK reasoning levels onto Anthropic thinking and effort.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L33)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicReasoning_IsCustom_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoning_IsCustom_System_String_"></a>

### IsCustom(string?)


```csharp
public static bool IsCustom(string? reasoning)
```


True when reasoning asks for a concrete mode rather than the provider default.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L36)

<a id="Vercel_AI_Anthropic_AnthropicReasoning_MapBudget_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoning_MapBudget_System_String_System_Int32_System_Collections_Generic_IList_Vercel_AI_Anthropic_AnthropicWarning__"></a>

### `MapBudget(string, int, IList<AnthropicWarning>)`


```csharp
public static int? MapBudget(string reasoning, int maxOutputTokens, IList<AnthropicWarning> warnings)
```


Token budget for budget-based thinking. Clamped to at least 1024.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxOutputTokens` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `warnings` | [IList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L127)

<a id="Vercel_AI_Anthropic_AnthropicReasoning_Resolve_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoning_Resolve_System_String_System_String_Vercel_AI_Anthropic_AnthropicModelCapabilities_System_Collections_Generic_IList_Vercel_AI_Anthropic_AnthropicWarning__"></a>

### `Resolve(string?, string, AnthropicModelCapabilities, IList<AnthropicWarning>)`


```csharp
public static AnthropicReasoningConfig? Resolve(string? reasoning, string modelId, AnthropicModelCapabilities capabilities, IList<AnthropicWarning> warnings)
```


Maps a reasoning level. Returns null for `provider-default`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `capabilities` | [AnthropicModelCapabilities](/api/Vercel.AI.Anthropic/AnthropicModelCapabilities/) |  |
| `warnings` | [IList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [AnthropicReasoningConfig](/api/Vercel.AI.Anthropic/AnthropicReasoningConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L42)
