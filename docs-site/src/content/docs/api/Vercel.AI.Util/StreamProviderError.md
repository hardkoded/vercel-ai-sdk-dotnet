---
title: "StreamProviderError"
description: "Error reported by a provider after a model response stream has started."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamProviderError"
  hidden: true
---

<a id="Vercel_AI_Util_StreamProviderError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class StreamProviderError : AiSdkError, ISerializable, IErrorMarkers
```


Error reported by a provider after a model response stream has started.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_StreamProviderError__ctor_"></a>
<a id="Vercel_AI_Util_StreamProviderError__ctor_System_String_System_String_System_Object_System_Nullable_System_Int32__System_Nullable_System_Boolean__System_Object_System_Exception_"></a>

### StreamProviderError(string, string?, object?, int?, bool?, object?, Exception?)


```csharp
public StreamProviderError(string message, string? type = null, object? code = null, int? statusCode = null, bool? isRetryable = null, object? data = null, Exception? cause = null)
```


Creates the error from provider metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L14)

## Fields

<a id="Vercel_AI_Util_StreamProviderError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_StreamProviderError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_StreamProviderError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L11)

## Properties

<a id="Vercel_AI_Util_StreamProviderError_Code_"></a>
<a id="Vercel_AI_Util_StreamProviderError_Code"></a>

### Code


```csharp
public object? Code { get; }
```


Provider-defined error code.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L36)

<a id="Vercel_AI_Util_StreamProviderError_Data_"></a>
<a id="Vercel_AI_Util_StreamProviderError_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Original provider error payload.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L45)

<a id="Vercel_AI_Util_StreamProviderError_IsRetryable_"></a>
<a id="Vercel_AI_Util_StreamProviderError_IsRetryable"></a>

### IsRetryable


```csharp
public bool IsRetryable { get; }
```


Whether retrying the model call may succeed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L42)

<a id="Vercel_AI_Util_StreamProviderError_StatusCode_"></a>
<a id="Vercel_AI_Util_StreamProviderError_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP-equivalent status code.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L39)

<a id="Vercel_AI_Util_StreamProviderError_Type_"></a>
<a id="Vercel_AI_Util_StreamProviderError_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Provider-defined error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L33)

## Methods

<a id="Vercel_AI_Util_StreamProviderError_IsInstance_"></a>
<a id="Vercel_AI_Util_StreamProviderError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L48)

<a id="Vercel_AI_Util_StreamProviderError_IsRetryableStatusCode_"></a>
<a id="Vercel_AI_Util_StreamProviderError_IsRetryableStatusCode_System_Nullable_System_Int32__"></a>

### IsRetryableStatusCode(int?)


```csharp
public static bool IsRetryableStatusCode(int? statusCode)
```


Retryable statuses are 408, 409, 429, and 500 and above.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StreamProviderError.cs#L54)
