---
title: "ReplicateVideoModelOptions"
description: "Replicate video provider options, read from providerOptions.replicate. A null property is the same as an omitted one. Options this class does not model are s..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReplicateVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions"></a>

Namespace: [Vercel.AI.Replicate](/api/Vercel.AI.Replicate/)


```csharp
public sealed class ReplicateVideoModelOptions
```


Replicate video provider options, read from `providerOptions.replicate`.
A null property is the same as an omitted one. Options this class does not model are sent as they are.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L14)

## Properties

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_AdditionalProperties_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; }
```


Options this class does not model, in order. They are sent to the API as they are. Upstream passthrough.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L53)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_CondAug_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_CondAug"></a>

### CondAug


```csharp
public double? CondAug { get; set; }
```


Stable Video Diffusion. Sent as `cond_aug`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L35)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_DecodingT_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_DecodingT"></a>

### DecodingT


```csharp
public double? DecodingT { get; set; }
```


Stable Video Diffusion. Sent as `decoding_t`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L38)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_FramesPerSecond_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_FramesPerSecond"></a>

### FramesPerSecond


```csharp
public double? FramesPerSecond { get; set; }
```


Stable Video Diffusion. Sent as `frames_per_second`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L47)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_GuidanceScale_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_GuidanceScale"></a>

### GuidanceScale


```csharp
public double? GuidanceScale { get; set; }
```


Sent as `guidance_scale`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L26)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_MaxWaitTimeInSeconds_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_MaxWaitTimeInSeconds"></a>

### MaxWaitTimeInSeconds


```csharp
public double? MaxWaitTimeInSeconds { get; set; }
```


Maximum wait in seconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L23)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_MotionBucketId_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_MotionBucketId"></a>

### MotionBucketId


```csharp
public double? MotionBucketId { get; set; }
```


Stable Video Diffusion. Sent as `motion_bucket_id`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L32)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_NumInferenceSteps_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_NumInferenceSteps"></a>

### NumInferenceSteps


```csharp
public double? NumInferenceSteps { get; set; }
```


Sent as `num_inference_steps`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L29)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_PollIntervalMs_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public double? PollIntervalMs { get; set; }
```


Poll interval in milliseconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L17)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_PollTimeoutMs_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public double? PollTimeoutMs { get; set; }
```


Poll timeout in milliseconds. Must be positive.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L20)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_PromptOptimizer_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_PromptOptimizer"></a>

### PromptOptimizer


```csharp
public bool? PromptOptimizer { get; set; }
```


MiniMax. Sent as `prompt_optimizer`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L50)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_SizingStrategy_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_SizingStrategy"></a>

### SizingStrategy


```csharp
public string? SizingStrategy { get; set; }
```


Stable Video Diffusion. Sent as `sizing_strategy`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L44)

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_VideoLength_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_VideoLength"></a>

### VideoLength


```csharp
public string? VideoLength { get; set; }
```


Stable Video Diffusion. Sent as `video_length`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L41)

## Methods

<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_Parse_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static ReplicateVideoModelOptions? Parse(JsonElement providerOptions)
```


Reads `providerOptions.replicate`. Returns null when absent. An invalid value throws [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [ReplicateVideoModelOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModelOptions.cs#L59)
