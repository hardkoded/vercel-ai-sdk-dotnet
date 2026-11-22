---
title: "ReplicateVideoModel"
description: "Replicate video model. CancellationToken) creates a prediction and CancellationToken) reads it. A model:version id posts to /predictions; other ids post to /..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReplicateVideoModel"
  hidden: true
---

<a id="Vercel_AI_Replicate_ReplicateVideoModel"></a>

Namespace: [Vercel.AI.Replicate](/api/Vercel.AI.Replicate/)


```csharp
public sealed class ReplicateVideoModel : IVideoModel, IVideoCaller
```


Replicate video model. `CancellationToken)` creates a prediction and `CancellationToken)` reads it.
A `model:version` id posts to `/predictions`; other ids post to `/models/{model}/predictions`.
The operation is a JSON object with the `getUrl` of the prediction.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L18)

## Constructors

<a id="Vercel_AI_Replicate_ReplicateVideoModel__ctor_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel__ctor_Vercel_AI_Replicate_ReplicateProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `ReplicateVideoModel(ReplicateProvider, string, Func<DateTimeOffset>?)`


```csharp
public ReplicateVideoModel(ReplicateProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ReplicateProvider](/api/Vercel.AI.Replicate/ReplicateProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L24)

## Properties

<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L48)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanStart_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L51)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanStatus_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L54)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


True. The webhook URL is sent as `webhook`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L57)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


Replicate video models create one video per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L45)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_ModelId_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L42)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_Provider_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L39)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L36)

## Methods

<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L87)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoCallOptions, CancellationToken)


```csharp
public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
```


Starts a prediction and polls it until the video is ready.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [VideoCallOptions](/api/Vercel.AI.Provider/VideoCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L66)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L108)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L157)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L99)

<a id="Vercel_AI_Replicate_ReplicateVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateVideoModel.cs#L60)
