---
title: "VideoStartResult"
description: "Result of doStart."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoStartResult"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoStartResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoStartResult
```


Result of `doStart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L283)

## Constructors

<a id="Vercel_AI_Operations_VideoStartResult__ctor_"></a>
<a id="Vercel_AI_Operations_VideoStartResult__ctor_System_Object_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `VideoStartResult(object?, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?)`


```csharp
public VideoStartResult(object? operation, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a start result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `operation` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L286)

## Properties

<a id="Vercel_AI_Operations_VideoStartResult_Operation_"></a>
<a id="Vercel_AI_Operations_VideoStartResult_Operation"></a>

### Operation


```csharp
public object? Operation { get; }
```


Opaque operation.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L295)

<a id="Vercel_AI_Operations_VideoStartResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_VideoStartResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L301)

<a id="Vercel_AI_Operations_VideoStartResult_Response_"></a>
<a id="Vercel_AI_Operations_VideoStartResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L304)

<a id="Vercel_AI_Operations_VideoStartResult_Warnings_"></a>
<a id="Vercel_AI_Operations_VideoStartResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L298)
