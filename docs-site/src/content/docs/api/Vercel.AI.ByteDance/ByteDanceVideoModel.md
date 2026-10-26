---
title: "ByteDanceVideoModel"
description: "ByteDance Seedance video model. CancellationToken) creates a task and CancellationToken) reads it. The operation is a JSON object with the taskId."
editUrl: false
prev: false
next: false
sidebar:
  label: "ByteDanceVideoModel"
  hidden: true
---

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel"></a>

Namespace: [Vercel.AI.ByteDance](/api/Vercel.AI.ByteDance/)


```csharp
public sealed class ByteDanceVideoModel : IVideoModel, IVideoCaller
```


ByteDance Seedance video model. `CancellationToken)` creates a task and `CancellationToken)` reads it.
The operation is a JSON object with the `taskId`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L17)

## Constructors

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel__ctor_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel__ctor_Vercel_AI_ByteDance_ByteDanceProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `ByteDanceVideoModel(ByteDanceProvider, string, Func<DateTimeOffset>?)`


```csharp
public ByteDanceVideoModel(ByteDanceProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ByteDanceProvider](/api/Vercel.AI.ByteDance/ByteDanceProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L75)

## Properties

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L95)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanStart_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L98)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanStatus_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L101)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


False. Progress notifications need a protocol-aware receiver, so a webhook URL is only forwarded as `callback_url`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L104)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


ByteDance creates one video per task.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L92)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_ModelId_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L89)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_Provider_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L86)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L83)

## Methods

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L121)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L113)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L127)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L157)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L216)

<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceVideoModel.cs#L107)
