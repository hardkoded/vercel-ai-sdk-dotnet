---
title: "ImageCallResult"
description: "One provider call preserved on the result."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageCallResult"
  hidden: true
---

<a id="Vercel_AI_Operations_ImageCallResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImageCallResult
```


One provider call preserved on the result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L195)

## Constructors

<a id="Vercel_AI_Operations_ImageCallResult__ctor_"></a>
<a id="Vercel_AI_Operations_ImageCallResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_GeneratedImage__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_OperationUsage_"></a>

### `ImageCallResult(IReadOnlyList<GeneratedImage>, JsonElement?, ProviderResponse, IReadOnlyList<OperationWarning>, OperationUsage?)`


```csharp
public ImageCallResult(IReadOnlyList<GeneratedImage> images, JsonElement? providerMetadata, ProviderResponse response, IReadOnlyList<OperationWarning> warnings, OperationUsage? usage)
```


Creates a call record.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `images` | [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L198)

## Properties

<a id="Vercel_AI_Operations_ImageCallResult_Images_"></a>
<a id="Vercel_AI_Operations_ImageCallResult_Images"></a>

### Images


```csharp
public IReadOnlyList<GeneratedImage> Images { get; }
```


Images from this call.

**Value:** [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L208)

<a id="Vercel_AI_Operations_ImageCallResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ImageCallResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata from this call.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L211)

<a id="Vercel_AI_Operations_ImageCallResult_Response_"></a>
<a id="Vercel_AI_Operations_ImageCallResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L214)

<a id="Vercel_AI_Operations_ImageCallResult_Usage_"></a>
<a id="Vercel_AI_Operations_ImageCallResult_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Usage from this call.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L220)

<a id="Vercel_AI_Operations_ImageCallResult_Warnings_"></a>
<a id="Vercel_AI_Operations_ImageCallResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from this call.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L217)
