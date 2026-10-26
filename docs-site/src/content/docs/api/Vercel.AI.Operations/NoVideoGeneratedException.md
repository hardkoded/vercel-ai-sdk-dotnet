---
title: "NoVideoGeneratedException"
description: "No video was produced."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoVideoGeneratedException"
  hidden: true
---

<a id="Vercel_AI_Operations_NoVideoGeneratedException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class NoVideoGeneratedException : AiSdkException, ISerializable
```


No video was produced.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L295)

## Constructors

<a id="Vercel_AI_Operations_NoVideoGeneratedException__ctor_"></a>
<a id="Vercel_AI_Operations_NoVideoGeneratedException__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__"></a>

### `NoVideoGeneratedException(IReadOnlyList<ProviderResponse>)`


```csharp
public NoVideoGeneratedException(IReadOnlyList<ProviderResponse> responses)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L298)

## Properties

<a id="Vercel_AI_Operations_NoVideoGeneratedException_Responses_"></a>
<a id="Vercel_AI_Operations_NoVideoGeneratedException_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Provider responses included with the failure.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L305)
