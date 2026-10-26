---
title: "NormalizedBatchRequest"
description: "Normalized request sent to doStartBatch."
editUrl: false
prev: false
next: false
sidebar:
  label: "NormalizedBatchRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_NormalizedBatchRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class NormalizedBatchRequest
```


Normalized request sent to `doStartBatch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L108)

## Constructors

<a id="Vercel_AI_Operations_NormalizedBatchRequest__ctor_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest__ctor_System_String_System_String_System_String_System_Text_Json_JsonElement_"></a>

### NormalizedBatchRequest(string, string, string, JsonElement)


```csharp
public NormalizedBatchRequest(string id, string type, string modelId, JsonElement options)
```


Creates a normalized request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L111)

## Properties

<a id="Vercel_AI_Operations_NormalizedBatchRequest_Id_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_Id"></a>

### Id


```csharp
public string Id { get; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L120)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L126)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_Options_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_Options"></a>

### Options


```csharp
public JsonElement Options { get; }
```


Normalized options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L129)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_Type_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_Type"></a>

### Type


```csharp
public string Type { get; }
```


Request type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L123)
