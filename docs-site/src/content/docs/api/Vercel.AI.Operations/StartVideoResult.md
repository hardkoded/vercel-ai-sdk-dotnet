---
title: "StartVideoResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "StartVideoResult"
  hidden: true
---

<a id="Vercel_AI_Operations_StartVideoResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StartVideoResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L458)

## Constructors

<a id="Vercel_AI_Operations_StartVideoResult__ctor_"></a>
<a id="Vercel_AI_Operations_StartVideoResult__ctor_System_Object_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `StartVideoResult(object?, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse)`


```csharp
public StartVideoResult(object? operation, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
```


Creates a start result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `operation` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L461)

## Properties

<a id="Vercel_AI_Operations_StartVideoResult_Operation_"></a>
<a id="Vercel_AI_Operations_StartVideoResult_Operation"></a>

### Operation


```csharp
public object? Operation { get; }
```


Opaque operation.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L470)

<a id="Vercel_AI_Operations_StartVideoResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_StartVideoResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L476)

<a id="Vercel_AI_Operations_StartVideoResult_Response_"></a>
<a id="Vercel_AI_Operations_StartVideoResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L479)

<a id="Vercel_AI_Operations_StartVideoResult_Warnings_"></a>
<a id="Vercel_AI_Operations_StartVideoResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Normalization warnings followed by start warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L473)
