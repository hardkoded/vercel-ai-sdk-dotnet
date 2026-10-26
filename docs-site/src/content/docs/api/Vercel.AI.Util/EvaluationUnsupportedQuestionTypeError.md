---
title: "EvaluationUnsupportedQuestionTypeError"
description: "Deprecated alias of DecisionUnsupportedQuestionTypeError. It carries the decision error marker."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationUnsupportedQuestionTypeError"
  hidden: true
---

<a id="Vercel_AI_Util_EvaluationUnsupportedQuestionTypeError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
[Obsolete("Use DecisionUnsupportedQuestionTypeError instead.")]
public sealed class EvaluationUnsupportedQuestionTypeError : DecisionUnsupportedQuestionTypeError, ISerializable, IErrorMarkers
```


Deprecated alias of [DecisionUnsupportedQuestionTypeError](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/). It carries the decision error marker.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/) → [DecisionUnsupportedQuestionTypeError](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L51)

## Constructors

<a id="Vercel_AI_Util_EvaluationUnsupportedQuestionTypeError__ctor_"></a>
<a id="Vercel_AI_Util_EvaluationUnsupportedQuestionTypeError__ctor_System_String_System_String_System_String_System_String_System_String_"></a>

### EvaluationUnsupportedQuestionTypeError(string, string, string, string, string?)


```csharp
public EvaluationUnsupportedQuestionTypeError(string questionId, string questionType, string provider, string modelId, string? message = null)
```


Creates the error for the unsupported question.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `questionType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L58)

## Fields

<a id="Vercel_AI_Util_EvaluationUnsupportedQuestionTypeError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_DecisionUnsupportedQuestionTypeError"
```


Marker. It equals [ErrorMarker](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/#Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_ErrorMarker).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L55)
