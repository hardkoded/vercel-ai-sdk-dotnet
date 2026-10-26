---
title: "BatchImageGeneration"
description: "A provider image generation inside a batch item."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchImageGeneration"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchImageGeneration"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchImageGeneration
```


A provider image generation inside a batch item.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L303)

## Properties

<a id="Vercel_AI_Operations_BatchImageGeneration_Images_"></a>
<a id="Vercel_AI_Operations_BatchImageGeneration_Images"></a>

### Images


```csharp
public IReadOnlyList<object?> Images { get; set; }
```


Image payloads.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L306)

<a id="Vercel_AI_Operations_BatchImageGeneration_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchImageGeneration_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L315)

<a id="Vercel_AI_Operations_BatchImageGeneration_Response_"></a>
<a id="Vercel_AI_Operations_BatchImageGeneration_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; set; }
```


Response.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L312)

<a id="Vercel_AI_Operations_BatchImageGeneration_Usage_"></a>
<a id="Vercel_AI_Operations_BatchImageGeneration_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; set; }
```


Usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L318)

<a id="Vercel_AI_Operations_BatchImageGeneration_Warnings_"></a>
<a id="Vercel_AI_Operations_BatchImageGeneration_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; set; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L309)
