---
title: "EvaluationUnsupportedQuestionTypeException"
description: "Deprecated alias of DecisionUnsupportedQuestionTypeException."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationUnsupportedQuestionTypeException"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluationUnsupportedQuestionTypeException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionUnsupportedQuestionTypeException instead.")]
public sealed class EvaluationUnsupportedQuestionTypeException : DecisionUnsupportedQuestionTypeException, ISerializable, IErrorMarkers
```


Deprecated alias of [DecisionUnsupportedQuestionTypeException](/api/Vercel.AI.Operations/DecisionUnsupportedQuestionTypeException/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [DecisionUnsupportedQuestionTypeException](/api/Vercel.AI.Operations/DecisionUnsupportedQuestionTypeException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L127)

## Constructors

<a id="Vercel_AI_Operations_EvaluationUnsupportedQuestionTypeException__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluationUnsupportedQuestionTypeException__ctor_System_String_System_String_System_String_System_String_"></a>

### EvaluationUnsupportedQuestionTypeException(string, string, string, string)


```csharp
public EvaluationUnsupportedQuestionTypeException(string questionId, string questionType, string provider, string modelId)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `questionType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L131)
