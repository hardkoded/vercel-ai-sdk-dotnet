---
title: "AnthropicEvaluationQuestion"
description: "One evaluation question."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicEvaluationQuestion"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicEvaluationQuestion
```


One evaluation question.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L14)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `AnthropicEvaluationQuestion(string, string, IReadOnlyList<string>?)`


```csharp
public AnthropicEvaluationQuestion(string name, string type, IReadOnlyList<string>? criteria)
```


Creates a question. &lt;code class="paramref"&gt;type&lt;/code&gt; is choice, score, or boolean.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `criteria` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L17)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion_Criteria_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion_Criteria"></a>

### Criteria


```csharp
public IReadOnlyList<string> Criteria { get; }
```


Choice labels or score level labels, in insertion order.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L31)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion_Name_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion_Name"></a>

### Name


```csharp
public string Name { get; }
```


Question name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L25)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion_Type_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationQuestion_Type"></a>

### Type


```csharp
public string Type { get; }
```


choice, score, or boolean.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L28)
