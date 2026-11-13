---
title: "TencentVideoModel"
description: "Tencent video model. CancellationToken) creates a task and CancellationToken) reads it. TokenHub Kling models use their TokenHub id (for example kling-video-..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentVideoModel"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentVideoModel"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentVideoModel : IVideoModel, IVideoCaller
```


Tencent video model. `CancellationToken)` creates a task and `CancellationToken)` reads it.
TokenHub Kling models use their TokenHub id (for example `kling-video-v3`); VOD AIGC models use `vod/<ModelName>/<ModelVersion>`.
The operation is a JSON object with the `taskId`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L19)

## Constructors

<a id="Vercel_AI_Tencent_TencentVideoModel__ctor_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel__ctor_Vercel_AI_Tencent_TencentProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `TencentVideoModel(TencentProvider, string, Func<DateTimeOffset>?)`


```csharp
public TencentVideoModel(TencentProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and the VOD signature time.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [TencentProvider](/api/Vercel.AI.Tencent/TencentProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L26)

## Properties

<a id="Vercel_AI_Tencent_TencentVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L47)

<a id="Vercel_AI_Tencent_TencentVideoModel_CanStart_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L50)

<a id="Vercel_AI_Tencent_TencentVideoModel_CanStatus_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L53)

<a id="Vercel_AI_Tencent_TencentVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


False. Tencent video tasks take no per-request callback URL.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L56)

<a id="Vercel_AI_Tencent_TencentVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


Tencent creates one video per task.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L44)

<a id="Vercel_AI_Tencent_TencentVideoModel_ModelId_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L41)

<a id="Vercel_AI_Tencent_TencentVideoModel_Provider_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L38)

<a id="Vercel_AI_Tencent_TencentVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L35)

## Methods

<a id="Vercel_AI_Tencent_TencentVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L73)

<a id="Vercel_AI_Tencent_TencentVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoCallOptions, CancellationToken)


```csharp
public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
```


Starts a task and polls it until the video is ready.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [VideoCallOptions](/api/Vercel.AI.Provider/VideoCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L65)

<a id="Vercel_AI_Tencent_TencentVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L85)

<a id="Vercel_AI_Tencent_TencentVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L152)

<a id="Vercel_AI_Tencent_TencentVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L79)

<a id="Vercel_AI_Tencent_TencentVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModel.cs#L59)
