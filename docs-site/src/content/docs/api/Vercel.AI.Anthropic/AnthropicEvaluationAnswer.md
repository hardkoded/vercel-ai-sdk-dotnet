---
title: "AnthropicEvaluationAnswer"
description: "One parsed answer."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicEvaluationAnswer"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicEvaluationAnswer
```


One parsed answer.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L35)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer__ctor_System_String_System_String_System_Nullable_System_Double__System_Nullable_System_Double__"></a>

### AnthropicEvaluationAnswer(string, string?, double?, double?)


```csharp
public AnthropicEvaluationAnswer(string type, string? choice, double? score, double? probability)
```


Creates an answer.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `choice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `score` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `probability` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L38)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Choice_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Choice"></a>

### Choice


```csharp
public string? Choice { get; }
```


Choice label.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L50)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Probability_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Probability"></a>

### Probability


```csharp
public double? Probability { get; }
```


Boolean probability.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L56)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Score_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Score"></a>

### Score


```csharp
public double? Score { get; }
```


Score.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L53)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Type_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationAnswer_Type"></a>

### Type


```csharp
public string Type { get; }
```


choice, score, or boolean.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L47)
