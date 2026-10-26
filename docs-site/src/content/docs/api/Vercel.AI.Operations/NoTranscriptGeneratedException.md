---
title: "NoTranscriptGeneratedException"
description: "No transcript text was produced."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoTranscriptGeneratedException"
  hidden: true
---

<a id="Vercel_AI_Operations_NoTranscriptGeneratedException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class NoTranscriptGeneratedException : AiSdkException, ISerializable
```


No transcript text was produced.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L267)

## Constructors

<a id="Vercel_AI_Operations_NoTranscriptGeneratedException__ctor_"></a>
<a id="Vercel_AI_Operations_NoTranscriptGeneratedException__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__"></a>

### `NoTranscriptGeneratedException(IReadOnlyList<ProviderResponse>)`


```csharp
public NoTranscriptGeneratedException(IReadOnlyList<ProviderResponse> responses)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L270)

## Properties

<a id="Vercel_AI_Operations_NoTranscriptGeneratedException_Responses_"></a>
<a id="Vercel_AI_Operations_NoTranscriptGeneratedException_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Provider responses included with the failure.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L277)
