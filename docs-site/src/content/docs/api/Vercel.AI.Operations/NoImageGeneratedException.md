---
title: "NoImageGeneratedException"
description: "No image was produced after retries."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoImageGeneratedException"
  hidden: true
---

<a id="Vercel_AI_Operations_NoImageGeneratedException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class NoImageGeneratedException : AiSdkException, ISerializable
```


No image was produced after retries.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L224)

## Constructors

<a id="Vercel_AI_Operations_NoImageGeneratedException__ctor_"></a>
<a id="Vercel_AI_Operations_NoImageGeneratedException__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ImageCallResult__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__"></a>

### `NoImageGeneratedException(IReadOnlyList<ImageCallResult>, IReadOnlyList<ProviderResponse>)`


```csharp
public NoImageGeneratedException(IReadOnlyList<ImageCallResult> calls, IReadOnlyList<ProviderResponse> responses)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `calls` | [IReadOnlyList&lt;ImageCallResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L227)

## Properties

<a id="Vercel_AI_Operations_NoImageGeneratedException_Calls_"></a>
<a id="Vercel_AI_Operations_NoImageGeneratedException_Calls"></a>

### Calls


```csharp
public IReadOnlyList<ImageCallResult> Calls { get; }
```


Calls that returned no usable image.

**Value:** [IReadOnlyList&lt;ImageCallResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L235)

<a id="Vercel_AI_Operations_NoImageGeneratedException_Responses_"></a>
<a id="Vercel_AI_Operations_NoImageGeneratedException_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Provider responses included with the failure.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L238)
