---
title: "DecisionUnsupportedQuestionTypeError"
description: "A decision model does not support the type of a requested question."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionUnsupportedQuestionTypeError"
  hidden: true
---

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public class DecisionUnsupportedQuestionTypeError : AiSdkError, ISerializable, IErrorMarkers
```


A decision model does not support the type of a requested question.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

**Derived:** [EvaluationUnsupportedQuestionTypeError](/api/Vercel.AI.Util/EvaluationUnsupportedQuestionTypeError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError__ctor_"></a>
<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError__ctor_System_String_System_String_System_String_System_String_System_String_"></a>

### DecisionUnsupportedQuestionTypeError(string, string, string, string, string?)


```csharp
public DecisionUnsupportedQuestionTypeError(string questionId, string questionType, string provider, string modelId, string? message = null)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L14)

## Fields

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_DecisionUnsupportedQuestionTypeError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L11)

## Properties

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_ModelId_"></a>
<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Decision model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L41)

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_Provider_"></a>
<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Decision provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L38)

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_QuestionId_"></a>
<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_QuestionId"></a>

### QuestionId


```csharp
public string QuestionId { get; }
```


Question identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L32)

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_QuestionType_"></a>
<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_QuestionType"></a>

### QuestionType


```csharp
public string QuestionType { get; }
```


Question type the model rejected.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L35)

## Methods

<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_IsInstance_"></a>
<a id="Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_IsInstance_System_Object_"></a>

### IsInstance(object?)


```csharp
public static bool IsInstance(object? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; carries this error's marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionUnsupportedQuestionTypeError.cs#L44)
