---
title: "RetryError"
description: "Retries were exhausted or stopped."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryError"
  hidden: true
---

<a id="Vercel_AI_Util_RetryError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class RetryError : AiSdkError, ISerializable, IErrorMarkers
```


Retries were exhausted or stopped.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L225)

## Constructors

<a id="Vercel_AI_Util_RetryError__ctor_"></a>
<a id="Vercel_AI_Util_RetryError__ctor_System_String_Vercel_AI_Util_RetryErrorReason_System_Collections_Generic_IReadOnlyList_System_Object__"></a>

### `RetryError(string, RetryErrorReason, IReadOnlyList<object>)`


```csharp
public RetryError(string message, RetryErrorReason reason, IReadOnlyList<object> errors)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reason` | [RetryErrorReason](/api/Vercel.AI.Util/RetryErrorReason/) |  |
| `errors` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L231)

## Fields

<a id="Vercel_AI_Util_RetryError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_RetryError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_RetryError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L228)

## Properties

<a id="Vercel_AI_Util_RetryError_Errors_"></a>
<a id="Vercel_AI_Util_RetryError_Errors"></a>

### Errors


```csharp
public IReadOnlyList<object> Errors { get; }
```


Errors from each attempt, in order.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L244)

<a id="Vercel_AI_Util_RetryError_LastError_"></a>
<a id="Vercel_AI_Util_RetryError_LastError"></a>

### LastError


```csharp
public object? LastError { get; }
```


Last attempt's error.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L247)

<a id="Vercel_AI_Util_RetryError_Reason_"></a>
<a id="Vercel_AI_Util_RetryError_Reason"></a>

### Reason


```csharp
public RetryErrorReason Reason { get; }
```


Why retrying stopped.

**Value:** [RetryErrorReason](/api/Vercel.AI.Util/RetryErrorReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L241)

## Methods

<a id="Vercel_AI_Util_RetryError_IsInstance_"></a>
<a id="Vercel_AI_Util_RetryError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L250)
