---
title: "ImageModelResult"
description: "One doGenerate result."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_ImageModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImageModelResult
```


One `doGenerate` result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L116)

## Constructors

<a id="Vercel_AI_Operations_ImageModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_ImageModelResult__ctor_System_Collections_Generic_IReadOnlyList_System_Object__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_OperationUsage_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_System_Nullable_System_Boolean__"></a>

### `ImageModelResult(IReadOnlyList<object?>, IReadOnlyList<OperationWarning>?, OperationUsage?, JsonElement?, ProviderResponse?, bool?)`


```csharp
public ImageModelResult(IReadOnlyList<object?> images, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, JsonElement? providerMetadata = null, ProviderResponse? response = null, bool? isRetryable = null)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `images` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L119)

## Properties

<a id="Vercel_AI_Operations_ImageModelResult_Images_"></a>
<a id="Vercel_AI_Operations_ImageModelResult_Images"></a>

### Images


```csharp
public IReadOnlyList<object?> Images { get; }
```


Image payloads: base64 strings or byte arrays.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L130)

<a id="Vercel_AI_Operations_ImageModelResult_IsRetryable_"></a>
<a id="Vercel_AI_Operations_ImageModelResult_IsRetryable"></a>

### IsRetryable


```csharp
public bool? IsRetryable { get; }
```


When false, an empty image list is terminal.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L145)

<a id="Vercel_AI_Operations_ImageModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ImageModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata, including per-image arrays.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L139)

<a id="Vercel_AI_Operations_ImageModelResult_Response_"></a>
<a id="Vercel_AI_Operations_ImageModelResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L142)

<a id="Vercel_AI_Operations_ImageModelResult_Usage_"></a>
<a id="Vercel_AI_Operations_ImageModelResult_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Token usage. Null when the provider omitted it.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L136)

<a id="Vercel_AI_Operations_ImageModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_ImageModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from this call.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L133)
