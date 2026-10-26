---
title: "VideoModelResult"
description: "Synchronous doGenerate result or a completed status payload."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoModelResult
```


Synchronous `doGenerate` result or a completed status payload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L258)

## Constructors

<a id="Vercel_AI_Operations_VideoModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_VideoModelResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoOutput__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `VideoModelResult(IReadOnlyList<VideoOutput>?, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?)`


```csharp
public VideoModelResult(IReadOnlyList<VideoOutput>? videos, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `videos` | [IReadOnlyList&lt;VideoOutput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L261)

## Properties

<a id="Vercel_AI_Operations_VideoModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_VideoModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L276)

<a id="Vercel_AI_Operations_VideoModelResult_Response_"></a>
<a id="Vercel_AI_Operations_VideoModelResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L279)

<a id="Vercel_AI_Operations_VideoModelResult_Videos_"></a>
<a id="Vercel_AI_Operations_VideoModelResult_Videos"></a>

### Videos


```csharp
public IReadOnlyList<VideoOutput> Videos { get; }
```


Videos.

**Value:** [IReadOnlyList&lt;VideoOutput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L270)

<a id="Vercel_AI_Operations_VideoModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_VideoModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L273)
