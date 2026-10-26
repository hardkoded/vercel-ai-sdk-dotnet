---
title: "GenerateVideoRequest"
description: "Options shared by generate and start."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateVideoRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateVideoRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class GenerateVideoRequest : OperationRequest
```


Options shared by generate and start.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L483)

## Properties

<a id="Vercel_AI_Operations_GenerateVideoRequest_AspectRatio_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L498)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Delay_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Delay"></a>

### Delay


```csharp
public Func<int, CancellationToken, Task>? Delay { get; set; }
```


Custom delay. Defaults to `CancellationToken)`.

**Value:** [Func&lt;int, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L534)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Download_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Download"></a>

### Download


```csharp
public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
```


Downloads URL videos and reference bytes are already local.

**Value:** [Func&lt;string, CancellationToken, Task&lt;DownloadedMedia&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L522)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Duration_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Duration"></a>

### Duration


```csharp
public double? Duration { get; set; }
```


Duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L504)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Fps_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Fps"></a>

### Fps


```csharp
public int? Fps { get; set; }
```


Frames per second.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L507)

<a id="Vercel_AI_Operations_GenerateVideoRequest_FrameImages_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_FrameImages"></a>

### FrameImages


```csharp
public IReadOnlyList<VideoFrameImage>? FrameImages { get; set; }
```


Frame images.

**Value:** [IReadOnlyList&lt;VideoFrameImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L513)

<a id="Vercel_AI_Operations_GenerateVideoRequest_GenerateAudio_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_GenerateAudio"></a>

### GenerateAudio


```csharp
public bool? GenerateAudio { get; set; }
```


Whether to generate audio.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L519)

<a id="Vercel_AI_Operations_GenerateVideoRequest_GenerateId_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_GenerateId"></a>

### GenerateId


```csharp
public Func<string>? GenerateId { get; set; }
```


Supplies idempotency suffixes. Default is a guid.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L543)

<a id="Vercel_AI_Operations_GenerateVideoRequest_InputReferences_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_InputReferences"></a>

### InputReferences


```csharp
public IReadOnlyList<VideoReference>? InputReferences { get; set; }
```


Input references.

**Value:** [IReadOnlyList&lt;VideoReference&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L516)

<a id="Vercel_AI_Operations_GenerateVideoRequest_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; set; }
```


Overrides the model limit.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L495)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Model_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Model"></a>

### Model


```csharp
public IVideoCaller? Model { get; set; }
```


Video model.

**Value:** [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L486)

<a id="Vercel_AI_Operations_GenerateVideoRequest_N_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_N"></a>

### N


```csharp
public double N { get; set; }
```


Number of videos. Default 1.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L492)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Poll_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Poll"></a>

### Poll


```csharp
public bool Poll { get; set; }
```


True when polling was requested.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L525)

<a id="Vercel_AI_Operations_GenerateVideoRequest_PollIntervalMs_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public int? PollIntervalMs { get; set; }
```


Poll interval. Default 5000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L528)

<a id="Vercel_AI_Operations_GenerateVideoRequest_PollTimeoutMs_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public int? PollTimeoutMs { get; set; }
```


Poll or webhook timeout. Default 600000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L531)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Prompt_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Prompt"></a>

### Prompt


```csharp
public VideoPrompt? Prompt { get; set; }
```


Prompt.

**Value:** [VideoPrompt](/api/Vercel.AI.Operations/VideoPrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L489)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Resolution_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Resolution"></a>

### Resolution


```csharp
public string? Resolution { get; set; }
```


Resolution.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L501)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Seed_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Seed"></a>

### Seed


```csharp
public double? Seed { get; set; }
```


Seed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L510)

<a id="Vercel_AI_Operations_GenerateVideoRequest_Webhook_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_Webhook"></a>

### Webhook


```csharp
public Func<Task<VideoWebhook>>? Webhook { get; set; }
```


Webhook factory.

**Value:** [Func&lt;Task&lt;VideoWebhook&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L537)

<a id="Vercel_AI_Operations_GenerateVideoRequest_WebhookUrl_"></a>
<a id="Vercel_AI_Operations_GenerateVideoRequest_WebhookUrl"></a>

### WebhookUrl


```csharp
public string? WebhookUrl { get; set; }
```


Webhook URL forwarded by [StartVideo](/api/Vercel.AI.Operations/StartVideo/).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L540)
