---
title: "AnthropicReasoningConfig"
description: "Thinking and effort derived from a top-level reasoning level."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicReasoningConfig"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicReasoningConfig
```


Thinking and effort derived from a top-level reasoning level.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L8)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig__ctor_System_String_System_Nullable_System_Int32__System_String_System_String_"></a>

### AnthropicReasoningConfig(string?, int?, string?, string?)


```csharp
public AnthropicReasoningConfig(string? thinkingType, int? budgetTokens, string? effort, string? display)
```


Creates a reasoning mapping.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `thinkingType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `budgetTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `effort` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `display` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L11)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_BudgetTokens_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_BudgetTokens"></a>

### BudgetTokens


```csharp
public int? BudgetTokens { get; }
```


Budget for `enabled` thinking.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L23)

<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_Display_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_Display"></a>

### Display


```csharp
public string? Display { get; }
```


Thinking display mode.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L29)

<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_Effort_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_Effort"></a>

### Effort


```csharp
public string? Effort { get; }
```


Effort level, when adaptive thinking uses one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L26)

<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_ThinkingType_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningConfig_ThinkingType"></a>

### ThinkingType


```csharp
public string? ThinkingType { get; }
```


`enabled`, `adaptive`, or `disabled`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicReasoning.cs#L20)
