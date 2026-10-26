---
title: "BatchRequest"
description: "One request in a batch."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchRequest
```


One request in a batch.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L36)

## Constructors

<a id="Vercel_AI_Operations_BatchRequest__ctor_"></a>
<a id="Vercel_AI_Operations_BatchRequest__ctor_System_String_System_String_System_String_"></a>

### BatchRequest(string, string, string)


```csharp
public BatchRequest(string id, string type, string model)
```


Creates a request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L39)

## Properties

<a id="Vercel_AI_Operations_BatchRequest_AspectRatio_"></a>
<a id="Vercel_AI_Operations_BatchRequest_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Image aspect ratio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L68)

<a id="Vercel_AI_Operations_BatchRequest_FrequencyPenalty_"></a>
<a id="Vercel_AI_Operations_BatchRequest_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L92)

<a id="Vercel_AI_Operations_BatchRequest_Id_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Id"></a>

### Id


```csharp
public string Id { get; }
```


Caller request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L47)

<a id="Vercel_AI_Operations_BatchRequest_ImagePrompt_"></a>
<a id="Vercel_AI_Operations_BatchRequest_ImagePrompt"></a>

### ImagePrompt


```csharp
public ImagePrompt? ImagePrompt { get; set; }
```


Image prompt.

**Value:** [ImagePrompt](/api/Vercel.AI.Operations/ImagePrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L59)

<a id="Vercel_AI_Operations_BatchRequest_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_BatchRequest_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L77)

<a id="Vercel_AI_Operations_BatchRequest_Model_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Model"></a>

### Model


```csharp
public string Model { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L53)

<a id="Vercel_AI_Operations_BatchRequest_N_"></a>
<a id="Vercel_AI_Operations_BatchRequest_N"></a>

### N


```csharp
public int? N { get; set; }
```


Image count.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L62)

<a id="Vercel_AI_Operations_BatchRequest_PresencePenalty_"></a>
<a id="Vercel_AI_Operations_BatchRequest_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L89)

<a id="Vercel_AI_Operations_BatchRequest_Prompt_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; set; }
```


Text prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L56)

<a id="Vercel_AI_Operations_BatchRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_BatchRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Per-request provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L74)

<a id="Vercel_AI_Operations_BatchRequest_Reasoning_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L98)

<a id="Vercel_AI_Operations_BatchRequest_Seed_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L71)

<a id="Vercel_AI_Operations_BatchRequest_Size_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Image size.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L65)

<a id="Vercel_AI_Operations_BatchRequest_StopSequences_"></a>
<a id="Vercel_AI_Operations_BatchRequest_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; set; }
```


Stop sequences.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L95)

<a id="Vercel_AI_Operations_BatchRequest_Temperature_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L80)

<a id="Vercel_AI_Operations_BatchRequest_ToolChoice_"></a>
<a id="Vercel_AI_Operations_BatchRequest_ToolChoice"></a>

### ToolChoice


```csharp
public string? ToolChoice { get; set; }
```


Tool choice such as `required`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L104)

<a id="Vercel_AI_Operations_BatchRequest_Tools_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Tools"></a>

### Tools


```csharp
public IReadOnlyList<BatchTool>? Tools { get; set; }
```


Tools. They are forwarded and not executed.

**Value:** [IReadOnlyList&lt;BatchTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L101)

<a id="Vercel_AI_Operations_BatchRequest_TopK_"></a>
<a id="Vercel_AI_Operations_BatchRequest_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Top-k.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L86)

<a id="Vercel_AI_Operations_BatchRequest_TopP_"></a>
<a id="Vercel_AI_Operations_BatchRequest_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Top-p.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L83)

<a id="Vercel_AI_Operations_BatchRequest_Type_"></a>
<a id="Vercel_AI_Operations_BatchRequest_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text`, `image`, or an unsupported type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L50)
