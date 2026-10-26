---
title: "AnthropicEvaluationModel"
description: "Builds and scores Anthropic evaluation requests on the Messages API."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicEvaluationModel"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicEvaluationModel : IRubricModel
```


Builds and scores Anthropic evaluation requests on the Messages API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L85)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel__ctor_Vercel_AI_Anthropic_AnthropicProvider_System_String_"></a>

### AnthropicEvaluationModel(AnthropicProvider, string)


```csharp
public AnthropicEvaluationModel(AnthropicProvider provider, string modelId)
```


Creates an evaluation model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L91)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_ModelId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L101)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L98)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model accepts.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L104)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_BuildSchema_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_BuildSchema_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicEvaluationQuestion__"></a>

### `BuildSchema(IReadOnlyList<AnthropicEvaluationQuestion>)`


```csharp
public static JsonElement BuildSchema(IReadOnlyList<AnthropicEvaluationQuestion> questions)
```


Builds the json schema. Choice values are `c0`, `c1`, …. Number bounds are omitted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questions` | [IReadOnlyList&lt;AnthropicEvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L196)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_DoEvaluateAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_DoEvaluateAsync_System_String_System_String_System_Threading_CancellationToken_"></a>

### DoEvaluateAsync(string, string, CancellationToken)


```csharp
public Task<RubricResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken)
```


Scores a candidate against a rubric.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `rubric` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `candidate` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RubricResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L107)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_EvaluateAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_EvaluateAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicEvaluationQuestion__Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### `EvaluateAsync(IReadOnlyList<AnthropicEvaluationQuestion>, LanguageModelCallOptions?, CancellationToken)`


```csharp
public Task<AnthropicEvaluationResult> EvaluateAsync(IReadOnlyList<AnthropicEvaluationQuestion> questions, LanguageModelCallOptions? options, CancellationToken cancellationToken)
```


Evaluates questions and maps `q0`, `q1`, … back onto the question names.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questions` | [IReadOnlyList&lt;AnthropicEvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AnthropicEvaluationResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L121)

<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_ParseAnswers_"></a>
<a id="Vercel_AI_Anthropic_AnthropicEvaluationModel_ParseAnswers_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicEvaluationQuestion__System_String_"></a>

### `ParseAnswers(IReadOnlyList<AnthropicEvaluationQuestion>, string)`


```csharp
public static IReadOnlyDictionary<string, AnthropicEvaluationAnswer> ParseAnswers(IReadOnlyList<AnthropicEvaluationQuestion> questions, string json)
```


Maps encoded answers back to criteria labels and checks score and probability bounds.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questions` | [IReadOnlyList&lt;AnthropicEvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, AnthropicEvaluationAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicEvaluation.cs#L233)
