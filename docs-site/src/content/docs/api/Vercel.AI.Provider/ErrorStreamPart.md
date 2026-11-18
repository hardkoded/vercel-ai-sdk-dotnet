---
title: "ErrorStreamPart"
description: "A stream error from the provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ErrorStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ErrorStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ErrorStreamPart : LanguageModelStreamPart
```


A stream error from the provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2926)

## Constructors

<a id="Vercel_AI_Provider_ErrorStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ErrorStreamPart__ctor_System_Exception_"></a>

### ErrorStreamPart(Exception)


```csharp
public ErrorStreamPart(Exception exception)
```


Creates an error part that carries the error object itself (upstream `error: unknown`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `exception` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2944)

<a id="Vercel_AI_Provider_ErrorStreamPart__ctor_System_String_"></a>

### ErrorStreamPart(string)


```csharp
public ErrorStreamPart(string message)
```


Creates an error part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2929)

<a id="Vercel_AI_Provider_ErrorStreamPart__ctor_Vercel_AI_Prompt_ProviderStreamError_"></a>

### ErrorStreamPart(ProviderStreamError)


```csharp
public ErrorStreamPart(ProviderStreamError error)
```


Creates an error part that carries the provider's structured error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [ProviderStreamError](/api/Vercel.AI.Prompt/ProviderStreamError/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2936)

## Properties

<a id="Vercel_AI_Provider_ErrorStreamPart_Error_"></a>
<a id="Vercel_AI_Provider_ErrorStreamPart_Error"></a>

### Error


```csharp
public ProviderStreamError? Error { get; }
```


The provider's structured error (`type`, `code`, status, retry flag, and the original `data`).
Null when the provider reported only a message.

**Value:** [ProviderStreamError](/api/Vercel.AI.Prompt/ProviderStreamError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2961)

<a id="Vercel_AI_Provider_ErrorStreamPart_Exception_"></a>
<a id="Vercel_AI_Provider_ErrorStreamPart_Exception"></a>

### Exception


```csharp
public Exception? Exception { get; }
```


The error object, such as an [ApiCallError](/api/Vercel.AI.Util/ApiCallError/) or a [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/). Null when the provider reported a message or a [Error](#Vercel_AI_Provider_ErrorStreamPart_Error).

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2955)

<a id="Vercel_AI_Provider_ErrorStreamPart_Message_"></a>
<a id="Vercel_AI_Provider_ErrorStreamPart_Message"></a>

### Message


```csharp
public string Message { get; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2952)
