---
title: "ProviderStreamError"
description: "A provider stream payload that already carries status metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderStreamError"
  hidden: true
---

<a id="Vercel_AI_Prompt_ProviderStreamError"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ProviderStreamError
```


A provider stream payload that already carries status metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L8)

## Constructors

<a id="Vercel_AI_Prompt_ProviderStreamError__ctor_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError__ctor_System_String_System_Object_System_String_System_Object_System_Nullable_System_Int32__System_Nullable_System_Boolean__"></a>

### ProviderStreamError(string, object?, string?, object?, int?, bool?)


```csharp
public ProviderStreamError(string message, object? data, string? type = null, object? code = null, int? statusCode = null, bool? isRetryable = null)
```


Creates a provider stream error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L11)

## Properties

<a id="Vercel_AI_Prompt_ProviderStreamError_Code_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError_Code"></a>

### Code


```csharp
public object? Code { get; }
```


Provider code.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L28)

<a id="Vercel_AI_Prompt_ProviderStreamError_Data_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Original provider data.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L37)

<a id="Vercel_AI_Prompt_ProviderStreamError_IsRetryable_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError_IsRetryable"></a>

### IsRetryable


```csharp
public bool? IsRetryable { get; }
```


Whether the caller should retry.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L34)

<a id="Vercel_AI_Prompt_ProviderStreamError_Message_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError_Message"></a>

### Message


```csharp
public string Message { get; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L22)

<a id="Vercel_AI_Prompt_ProviderStreamError_StatusCode_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L31)

<a id="Vercel_AI_Prompt_ProviderStreamError_Type_"></a>
<a id="Vercel_AI_Prompt_ProviderStreamError_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Provider type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L25)
