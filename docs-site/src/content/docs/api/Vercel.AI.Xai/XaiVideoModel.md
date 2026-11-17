---
title: "XaiVideoModel"
description: "xAI video model. CancellationToken) creates a request and CancellationToken) reads it. It supports generation, editing, extension, and reference-to-video, an..."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiVideoModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiVideoModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiVideoModel : IVideoModel, IVideoCaller
```


xAI video model. `CancellationToken)` creates a request and `CancellationToken)` reads it.
It supports generation, editing, extension, and reference-to-video, and can deliver the video to a signed upload URL.
The operation is a JSON object with the `requestId` and, for upload delivery, `hasUploadUrl`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L18)

## Constructors

<a id="Vercel_AI_Xai_XaiVideoModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel__ctor_Vercel_AI_Xai_XaiProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `XaiVideoModel(XaiProvider, string, Func<DateTimeOffset>?)`


```csharp
public XaiVideoModel(XaiProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L37)

## Properties

<a id="Vercel_AI_Xai_XaiVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L57)

<a id="Vercel_AI_Xai_XaiVideoModel_CanStart_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L60)

<a id="Vercel_AI_Xai_XaiVideoModel_CanStatus_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L63)

<a id="Vercel_AI_Xai_XaiVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


False. xAI does not send webhooks to this model.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L66)

<a id="Vercel_AI_Xai_XaiVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


xAI creates one video per request.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L54)

<a id="Vercel_AI_Xai_XaiVideoModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L51)

<a id="Vercel_AI_Xai_XaiVideoModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L48)

<a id="Vercel_AI_Xai_XaiVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L45)

## Methods

<a id="Vercel_AI_Xai_XaiVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L96)

<a id="Vercel_AI_Xai_XaiVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoCallOptions, CancellationToken)


```csharp
public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
```


Starts a request and polls it until the video is ready.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [VideoCallOptions](/api/Vercel.AI.Provider/VideoCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L75)

<a id="Vercel_AI_Xai_XaiVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L114)

<a id="Vercel_AI_Xai_XaiVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L181)

<a id="Vercel_AI_Xai_XaiVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L105)

<a id="Vercel_AI_Xai_XaiVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_Xai_XaiVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModel.cs#L69)
