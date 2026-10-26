---
title: "GenerateImageResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateImageResult"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateImageResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateImageResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L242)

## Constructors

<a id="Vercel_AI_Operations_GenerateImageResult__ctor_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_GeneratedImage__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ImageCallResult__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__System_Text_Json_JsonElement_Vercel_AI_Operations_OperationUsage_"></a>

### `GenerateImageResult(IReadOnlyList<GeneratedImage>, IReadOnlyList<ImageCallResult>, IReadOnlyList<OperationWarning>, IReadOnlyList<ProviderResponse>, JsonElement, OperationUsage)`


```csharp
public GenerateImageResult(IReadOnlyList<GeneratedImage> images, IReadOnlyList<ImageCallResult> calls, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata, OperationUsage usage)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `images` | [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `calls` | [IReadOnlyList&lt;ImageCallResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L245)

## Properties

<a id="Vercel_AI_Operations_GenerateImageResult_Calls_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_Calls"></a>

### Calls


```csharp
public IReadOnlyList<ImageCallResult> Calls { get; }
```


Underlying calls, including empty retries that were later replaced.

**Value:** [IReadOnlyList&lt;ImageCallResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L265)

<a id="Vercel_AI_Operations_GenerateImageResult_Image_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_Image"></a>

### Image


```csharp
public GeneratedImage Image { get; }
```


First image.

**Value:** [GeneratedImage](/api/Vercel.AI.Operations/GeneratedImage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L256)

<a id="Vercel_AI_Operations_GenerateImageResult_Images_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_Images"></a>

### Images


```csharp
public IReadOnlyList<GeneratedImage> Images { get; }
```


All images.

**Value:** [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L262)

<a id="Vercel_AI_Operations_GenerateImageResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Merged provider metadata.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L274)

<a id="Vercel_AI_Operations_GenerateImageResult_Responses_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


One response per collected call.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L271)

<a id="Vercel_AI_Operations_GenerateImageResult_Usage_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Summed usage. Counts stay null until a provider reports them.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L277)

<a id="Vercel_AI_Operations_GenerateImageResult_Warnings_"></a>
<a id="Vercel_AI_Operations_GenerateImageResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Aggregated warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L268)
