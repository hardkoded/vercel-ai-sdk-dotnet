---
title: "StreamProviderError"
description: "A normalized stream provider error. Maps to StreamProviderError."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamProviderError"
  hidden: true
---

<a id="Vercel_AI_Prompt_StreamProviderError"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class StreamProviderError : Exception, ISerializable
```


A normalized stream provider error. Maps to `StreamProviderError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L41)

## Constructors

<a id="Vercel_AI_Prompt_StreamProviderError__ctor_"></a>
<a id="Vercel_AI_Prompt_StreamProviderError__ctor_System_String_System_String_System_Object_System_Nullable_System_Int32__System_Boolean_System_Object_"></a>

### StreamProviderError(string, string?, object?, int?, bool, object?)


```csharp
public StreamProviderError(string message, string? type, object? code, int? statusCode, bool isRetryable, object? data)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `isRetryable` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L44)

## Properties

<a id="Vercel_AI_Prompt_StreamProviderError_Code_"></a>
<a id="Vercel_AI_Prompt_StreamProviderError_Code"></a>

### Code


```csharp
public object? Code { get; }
```


Provider code.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L58)

<a id="Vercel_AI_Prompt_StreamProviderError_Data_"></a>
<a id="Vercel_AI_Prompt_StreamProviderError_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Original payload.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L67)

<a id="Vercel_AI_Prompt_StreamProviderError_IsRetryable_"></a>
<a id="Vercel_AI_Prompt_StreamProviderError_IsRetryable"></a>

### IsRetryable


```csharp
public bool IsRetryable { get; }
```


Whether the caller should retry.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L64)

<a id="Vercel_AI_Prompt_StreamProviderError_StatusCode_"></a>
<a id="Vercel_AI_Prompt_StreamProviderError_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L61)

<a id="Vercel_AI_Prompt_StreamProviderError_Type_"></a>
<a id="Vercel_AI_Prompt_StreamProviderError_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Provider type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L55)
