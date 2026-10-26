---
title: "AnthropicEvaluationResult"
description: "Evaluation call result."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicEvaluationResult"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicEvaluationResult
```


Evaluation call result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L60)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Anthropic_AnthropicEvaluationAnswer__Vercel_AI_Provider_LanguageModelUsage_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `AnthropicEvaluationResult(IReadOnlyDictionary<string, AnthropicEvaluationAnswer>, LanguageModelUsage, IReadOnlyDictionary<string, string>, string)`


```csharp
public AnthropicEvaluationResult(IReadOnlyDictionary<string, AnthropicEvaluationAnswer> answers, LanguageModelUsage usage, IReadOnlyDictionary<string, string> headers, string provider)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `answers` | [IReadOnlyDictionary&lt;string, AnthropicEvaluationAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L63)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Answers_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, AnthropicEvaluationAnswer> Answers { get; }
```


Answers keyed by question name.

**Value:** [IReadOnlyDictionary&lt;string, AnthropicEvaluationAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L72)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Headers_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L78)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Always `anthropic.evaluation` for the model that produced this result.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L81)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Usage_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationResult_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Converted token usage.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L75)
