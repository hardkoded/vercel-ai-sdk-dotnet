---
title: "GenerateVideoResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateVideoResult"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateVideoResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[JsonConverter(typeof(GenerateVideoResultConverter))]
public sealed class GenerateVideoResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L493)

## Constructors

<a id="Vercel_AI_Operations_GenerateVideoResult__ctor_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_GeneratedVideo__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__System_Text_Json_JsonElement_System_Int32_"></a>

### `GenerateVideoResult(IReadOnlyList<GeneratedVideo>, IReadOnlyList<OperationWarning>, IReadOnlyList<ProviderResponse>, JsonElement, int)`


```csharp
public GenerateVideoResult(IReadOnlyList<GeneratedVideo> videos, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata, int uploadedVideos = 0)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `videos` | [IReadOnlyList&lt;GeneratedVideo&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `uploadedVideos` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L497)

## Properties

<a id="Vercel_AI_Operations_GenerateVideoResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Merged provider metadata.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L545)

<a id="Vercel_AI_Operations_GenerateVideoResult_Responses_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Responses. Each one carries that call's provider metadata.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L542)

<a id="Vercel_AI_Operations_GenerateVideoResult_UploadedVideos_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult_UploadedVideos"></a>

### UploadedVideos


```csharp
public int UploadedVideos { get; }
```


Number of videos delivered directly to caller-managed storage. They are not downloaded and are not in [Videos](#Vercel_AI_Operations_GenerateVideoResult_Videos).

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L536)

<a id="Vercel_AI_Operations_GenerateVideoResult_Video_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult_Video"></a>

### Video


```csharp
public GeneratedVideo Video { get; }
```


The first video available to the SDK.
Throws [NoVideoGeneratedException](/api/Vercel.AI.Operations/NoVideoGeneratedException/) when all videos were delivered directly to caller-managed storage. Use [UploadedVideos](#Vercel_AI_Operations_GenerateVideoResult_UploadedVideos) for upload-only results.

**Value:** [GeneratedVideo](/api/Vercel.AI.Operations/GeneratedVideo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L516)

<a id="Vercel_AI_Operations_GenerateVideoResult_Videos_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult_Videos"></a>

### Videos


```csharp
public IReadOnlyList<GeneratedVideo> Videos { get; }
```


All videos available to the SDK, excluding videos uploaded by the provider.

**Value:** [IReadOnlyList&lt;GeneratedVideo&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L533)

<a id="Vercel_AI_Operations_GenerateVideoResult_Warnings_"></a>
<a id="Vercel_AI_Operations_GenerateVideoResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L539)
