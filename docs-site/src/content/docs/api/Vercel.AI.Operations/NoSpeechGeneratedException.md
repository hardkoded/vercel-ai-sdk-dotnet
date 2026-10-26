---
title: "NoSpeechGeneratedException"
description: "No speech audio was produced."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoSpeechGeneratedException"
  hidden: true
---

<a id="Vercel_AI_Operations_NoSpeechGeneratedException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class NoSpeechGeneratedException : AiSdkException, ISerializable
```


No speech audio was produced.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L247)

## Constructors

<a id="Vercel_AI_Operations_NoSpeechGeneratedException__ctor_"></a>
<a id="Vercel_AI_Operations_NoSpeechGeneratedException__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__"></a>

### `NoSpeechGeneratedException(IReadOnlyList<ProviderResponse>)`


```csharp
public NoSpeechGeneratedException(IReadOnlyList<ProviderResponse> responses)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L250)

<a id="Vercel_AI_Operations_NoSpeechGeneratedException__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__System_Exception_"></a>

### `NoSpeechGeneratedException(IReadOnlyList<ProviderResponse>, Exception?)`


```csharp
public NoSpeechGeneratedException(IReadOnlyList<ProviderResponse> responses, Exception? cause)
```


Creates the exception with the provider error that caused it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L256)

## Properties

<a id="Vercel_AI_Operations_NoSpeechGeneratedException_Responses_"></a>
<a id="Vercel_AI_Operations_NoSpeechGeneratedException_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Provider responses included with the failure.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L263)
