---
title: "DecisionRefusalError"
description: "A decision model refused one or more questions."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionRefusalError"
  hidden: true
---

<a id="Vercel_AI_Util_DecisionRefusalError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DecisionRefusalError : AiSdkError, ISerializable, IErrorMarkers
```


A decision model refused one or more questions.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L11)

## Constructors

<a id="Vercel_AI_Util_DecisionRefusalError__ctor_"></a>
<a id="Vercel_AI_Util_DecisionRefusalError__ctor_System_Collections_Generic_IReadOnlyList_System_String__System_String_System_String_System_String_"></a>

### `DecisionRefusalError(IReadOnlyList<string>, string, string, string?)`


```csharp
public DecisionRefusalError(IReadOnlyList<string> questionIds, string provider, string modelId, string? message = null)
```


Creates the error for the refused questions.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questionIds` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L19)

## Fields

<a id="Vercel_AI_Util_DecisionRefusalError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_DecisionRefusalError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_DecisionRefusalError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L14)

## Properties

<a id="Vercel_AI_Util_DecisionRefusalError_ModelId_"></a>
<a id="Vercel_AI_Util_DecisionRefusalError_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Decision model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L41)

<a id="Vercel_AI_Util_DecisionRefusalError_Provider_"></a>
<a id="Vercel_AI_Util_DecisionRefusalError_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Decision provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L38)

<a id="Vercel_AI_Util_DecisionRefusalError_QuestionIds_"></a>
<a id="Vercel_AI_Util_DecisionRefusalError_QuestionIds"></a>

### QuestionIds


```csharp
public IReadOnlyList<string> QuestionIds { get; }
```


Refused question ids, in question order.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L35)

## Methods

<a id="Vercel_AI_Util_DecisionRefusalError_IsInstance_"></a>
<a id="Vercel_AI_Util_DecisionRefusalError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DecisionRefusalError.cs#L44)
