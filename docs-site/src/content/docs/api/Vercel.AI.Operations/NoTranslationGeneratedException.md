---
title: "NoTranslationGeneratedException"
description: "No translation text or audio was produced."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoTranslationGeneratedException"
  hidden: true
---

<a id="Vercel_AI_Operations_NoTranslationGeneratedException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class NoTranslationGeneratedException : AiSdkException, ISerializable
```


No translation text or audio was produced.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L281)

## Constructors

<a id="Vercel_AI_Operations_NoTranslationGeneratedException__ctor_"></a>
<a id="Vercel_AI_Operations_NoTranslationGeneratedException__ctor_Vercel_AI_Operations_ProviderResponse_"></a>

### NoTranslationGeneratedException(ProviderResponse)


```csharp
public NoTranslationGeneratedException(ProviderResponse response)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L284)

## Properties

<a id="Vercel_AI_Operations_NoTranslationGeneratedException_Response_"></a>
<a id="Vercel_AI_Operations_NoTranslationGeneratedException_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Provider response included with the failure.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L291)
