---
title: "HeyGenVideoModel"
description: "HeyGen video model. CancellationToken) submits a job and CancellationToken) reads it. The operation is a JSON object with videoId, mode and resolution. The r..."
editUrl: false
prev: false
next: false
sidebar:
  label: "HeyGenVideoModel"
  hidden: true
---

<a id="Vercel_AI_HeyGen_HeyGenVideoModel"></a>

Namespace: [Vercel.AI.HeyGen](/api/Vercel.AI.HeyGen/)


```csharp
public sealed class HeyGenVideoModel : IVideoModel, IVideoCaller
```


HeyGen video model. `CancellationToken)` submits a job and `CancellationToken)` reads it.
The operation is a JSON object with `videoId`, `mode` and `resolution`.
The result is the signed video URL. The bytes are not downloaded.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L18)

## Constructors

<a id="Vercel_AI_HeyGen_HeyGenVideoModel__ctor_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel__ctor_Vercel_AI_HeyGen_HeyGenProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `HeyGenVideoModel(HeyGenProvider, string, Func<DateTimeOffset>?)`


```csharp
public HeyGenVideoModel(HeyGenProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [HeyGenProvider](/api/Vercel.AI.HeyGen/HeyGenProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L50)

## Properties

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L70)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanStart_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L73)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanStatus_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L76)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


False. HeyGen is polled.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L79)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


HeyGen creates one video per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L67)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_ModelId_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L64)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_Provider_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L61)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L58)

## Methods

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoModelCall, CancellationToken)


```csharp
public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
```


Generates videos in one call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [VideoModelCall](/api/Vercel.AI.Operations/VideoModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L96)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoCallOptions, CancellationToken)


```csharp
public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
```


Starts a job and polls it until the video is ready.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [VideoCallOptions](/api/Vercel.AI.Provider/VideoCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L88)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

### DoStartAsync(VideoModelCall, CancellationToken)


```csharp
public Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
```


Starts an asynchronous generation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [VideoModelCall](/api/Vercel.AI.Operations/VideoModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoStartResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L108)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

### DoStatusAsync(VideoModelCall, CancellationToken)


```csharp
public Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
```


Reads operation status.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [VideoModelCall](/api/Vercel.AI.Operations/VideoModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoStatusResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L147)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

### `HandleWebhookAsync(Func<Task<VideoWebhook>>, CancellationToken)`


```csharp
public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
```


Turns a webhook factory into a URL and a received task.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `webhook` | [Func&lt;Task&lt;VideoWebhook&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoWebhook&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L102)

<a id="Vercel_AI_HeyGen_HeyGenVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_HeyGen_HeyGenVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

### ResolveMaxVideosPerCallAsync(CancellationToken)


```csharp
public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
```


Resolves a functional limit. The default is [MaxVideosPerCall](/api/Vercel.AI.Operations/IVideoCaller/#Vercel_AI_Operations_IVideoCaller_MaxVideosPerCall).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;int?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenVideoModel.cs#L82)
