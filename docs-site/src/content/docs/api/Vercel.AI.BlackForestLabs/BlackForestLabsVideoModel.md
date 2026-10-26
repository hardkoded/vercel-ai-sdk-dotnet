---
title: "BlackForestLabsVideoModel"
description: "Black Forest Labs FLUX 3 video model. CancellationToken) submits a job and CancellationToken) reads it. The video is returned as a signed URL and is never do..."
editUrl: false
prev: false
next: false
sidebar:
  label: "BlackForestLabsVideoModel"
  hidden: true
---

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel"></a>

Namespace: [Vercel.AI.BlackForestLabs](/api/Vercel.AI.BlackForestLabs/)


```csharp
public sealed class BlackForestLabsVideoModel : IVideoModel, IVideoCaller
```


Black Forest Labs FLUX 3 video model. `CancellationToken)` submits a job and `CancellationToken)` reads it.
The video is returned as a signed URL and is never downloaded. The operation is a JSON object with the
`requestId`, the `pollingUrl`, and the estimates of the submit response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L23)

## Constructors

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel__ctor_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel__ctor_Vercel_AI_BlackForestLabs_BlackForestLabsProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `BlackForestLabsVideoModel(BlackForestLabsProvider, string, Func<DateTimeOffset>?)`


```csharp
public BlackForestLabsVideoModel(BlackForestLabsProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [BlackForestLabsProvider](/api/Vercel.AI.BlackForestLabs/BlackForestLabsProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L43)

## Properties

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L63)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanStart_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L66)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanStatus_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L69)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


True when `handleWebhookOption` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L72)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


Black Forest Labs returns one video per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L60)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_ModelId_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L57)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_Provider_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L54)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L51)

## Methods

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoModelCall, CancellationToken)


```csharp
public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
```


Starts a job and polls it until the video is ready.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [VideoModelCall](/api/Vercel.AI.Operations/VideoModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L90)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoCallOptions, CancellationToken)


```csharp
public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
```


Generates a video.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [VideoCallOptions](/api/Vercel.AI.Provider/VideoCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L81)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L127)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L162)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L242)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsVideoModel.cs#L75)
