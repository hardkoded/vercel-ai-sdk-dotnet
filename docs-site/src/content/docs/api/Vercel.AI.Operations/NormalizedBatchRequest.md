---
title: "NormalizedBatchRequest"
description: "Normalized request sent to doStartBatch. Maps to BatchV4Request."
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


Normalized request sent to `doStartBatch`. Maps to `BatchV4Request`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L145)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L148)

## Properties

<a id="Vercel_AI_Operations_NormalizedBatchRequest_Id_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_Id"></a>

### Id


```csharp
public string Id { get; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L157)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L163)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_Options_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_Options"></a>

### Options


```csharp
public JsonElement Options { get; }
```


Normalized options as JSON. [ToTextCallOptions(string)](#Vercel_AI_Operations_NormalizedBatchRequest_ToTextCallOptions_System_String_) and [ToImageCallOptions()](#Vercel_AI_Operations_NormalizedBatchRequest_ToImageCallOptions) give the typed view.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L166)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_Type_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_Type"></a>

### Type


```csharp
public string Type { get; }
```


Request type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L160)

## Methods

<a id="Vercel_AI_Operations_NormalizedBatchRequest_ToImageCallOptions_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_ToImageCallOptions"></a>

### ToImageCallOptions()


```csharp
public ImageCallOptions ToImageCallOptions()
```


Typed view of [Options](#Vercel_AI_Operations_NormalizedBatchRequest_Options) for an image request. Upstream types `options` as a `Pick` of
`ImageModelV4CallOptions`: prompt, n, size, aspectRatio, seed, files, mask, and providerOptions.

**Returns:** [ImageCallOptions](/api/Vercel.AI.Provider/ImageCallOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L191)

<a id="Vercel_AI_Operations_NormalizedBatchRequest_ToTextCallOptions_"></a>
<a id="Vercel_AI_Operations_NormalizedBatchRequest_ToTextCallOptions_System_String_"></a>

### ToTextCallOptions(string)


```csharp
public LanguageModelCallOptions ToTextCallOptions(string providerName = "OpenAI")
```


Typed view of [Options](#Vercel_AI_Operations_NormalizedBatchRequest_Options) for a text request. Upstream types `options` as a `Pick` of
`LanguageModelV4CallOptions`: prompt, maxOutputTokens, temperature, stopSequences, topP, topK,
presencePenalty, frequencyPenalty, seed, reasoning, responseFormat, toolChoice, tools, and providerOptions.
Never carries headers, an abort signal, or raw chunks.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Provider named in the error for an unsupported tool or prompt part. |

**Returns:** [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L175)
