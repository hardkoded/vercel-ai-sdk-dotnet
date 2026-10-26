---
title: "DecisionUnsupportedQuestionTypeException"
description: "The decision model does not support a question type. It carries the ErrorMarker marker, so IsInstance(object) recognizes it."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionUnsupportedQuestionTypeException"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionUnsupportedQuestionTypeException : AiSdkException, ISerializable, IErrorMarkers
```


The decision model does not support a question type. It carries the
[ErrorMarker](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/#Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_ErrorMarker) marker, so
[IsInstance(object)](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/#Vercel_AI_Util_DecisionUnsupportedQuestionTypeError_IsInstance_System_Object_) recognizes it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

**Derived:** [EvaluationUnsupportedQuestionTypeException](/api/Vercel.AI.Operations/EvaluationUnsupportedQuestionTypeException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L331)

## Constructors

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException__ctor_System_String_System_String_System_String_System_String_"></a>

### DecisionUnsupportedQuestionTypeException(string, string, string, string)


```csharp
public DecisionUnsupportedQuestionTypeException(string questionId, string questionType, string provider, string modelId)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `questionType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L334)

## Properties

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_ErrorName_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_ErrorName"></a>

### ErrorName


```csharp
public string ErrorName { get; }
```


Upstream error name, `AI_DecisionUnsupportedQuestionTypeError`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L356)

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_ModelId_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L353)

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_Provider_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L350)

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_QuestionId_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_QuestionId"></a>

### QuestionId


```csharp
public string QuestionId { get; }
```


Question id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L344)

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_QuestionType_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_QuestionType"></a>

### QuestionType


```csharp
public string QuestionType { get; }
```


Rejected question type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L347)

## Methods

<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_HasMarker_"></a>
<a id="Vercel_AI_Operations_DecisionUnsupportedQuestionTypeException_HasMarker_System_String_"></a>

### HasMarker(string)


```csharp
public bool HasMarker(string marker)
```


Returns whether &lt;code class="paramref"&gt;marker&lt;/code&gt; is set to `true`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `marker` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L359)
