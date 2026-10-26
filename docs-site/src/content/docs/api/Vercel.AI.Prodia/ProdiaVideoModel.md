---
title: "ProdiaVideoModel"
description: "Prodia video model. A prompt alone sends a JSON job (text to video). An input image sends a multipart job (image to video). Image URLs are checked against pr..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProdiaVideoModel"
  hidden: true
---

<a id="Vercel_AI_Prodia_ProdiaVideoModel"></a>

Namespace: [Vercel.AI.Prodia](/api/Vercel.AI.Prodia/)


```csharp
public sealed class ProdiaVideoModel : IVideoModel, IVideoCaller
```


Prodia video model. A prompt alone sends a JSON job (text to video). An input image sends a multipart job
(image to video). Image URLs are checked against private addresses before they are downloaded.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L17)

## Constructors

<a id="Vercel_AI_Prodia_ProdiaVideoModel__ctor_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel__ctor_Vercel_AI_Prodia_ProdiaProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `ProdiaVideoModel(ProdiaProvider, string, Func<DateTimeOffset>?)`


```csharp
public ProdiaVideoModel(ProdiaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ProdiaProvider](/api/Vercel.AI.Prodia/ProdiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L23)

## Properties

<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L43)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanStart_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L46)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanStatus_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L49)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


True when `handleWebhookOption` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L52)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


Prodia returns one video per job.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L40)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_ModelId_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L37)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_Provider_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L34)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L31)

## Methods

<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L70)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L61)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L144)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L150)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L156)

<a id="Vercel_AI_Prodia_ProdiaVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaVideoModel.cs#L55)
