---
title: "GatewayVideoModel"
description: "Gateway video model. CancellationToken) reads the first data event of an SSE response from /video-model. CancellationToken) and CancellationToken) use /video..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayVideoModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayVideoModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayVideoModel : IVideoModel, IVideoCaller
```


Gateway video model. `CancellationToken)` reads the first data event of an
SSE response from `/video-model`. `CancellationToken)` and `CancellationToken)` use
`/video-model/start` and `/video-model/status`. Maps to `GatewayVideoModel` in `gateway-video-model.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L18)

## Constructors

<a id="Vercel_AI_Gateway_GatewayVideoModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel__ctor_Vercel_AI_Gateway_GatewayProvider_System_String_System_String_"></a>

### GatewayVideoModel(GatewayProvider, string, string?)


```csharp
public GatewayVideoModel(GatewayProvider provider, string modelId, string? providerName = null)
```


Creates a Gateway video model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) | Gateway provider that sends the requests. |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Model id. |
| `providerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Value of [Provider](#Vercel_AI_Gateway_GatewayVideoModel_Provider) (upstream `GatewayConfig.provider`). Defaults to `gateway`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L26)

## Properties

<a id="Vercel_AI_Gateway_GatewayVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L46)

<a id="Vercel_AI_Gateway_GatewayVideoModel_CanStart_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L49)

<a id="Vercel_AI_Gateway_GatewayVideoModel_CanStatus_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L52)

<a id="Vercel_AI_Gateway_GatewayVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


True when `handleWebhookOption` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L55)

<a id="Vercel_AI_Gateway_GatewayVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


Large so that requests are never split client-side.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L43)

<a id="Vercel_AI_Gateway_GatewayVideoModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L40)

<a id="Vercel_AI_Gateway_GatewayVideoModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L37)

<a id="Vercel_AI_Gateway_GatewayVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L34)

## Methods

<a id="Vercel_AI_Gateway_GatewayVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(VideoModelCall, CancellationToken)


```csharp
public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
```


Sends the call and reads the first data event of the SSE response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [VideoModelCall](/api/Vercel.AI.Operations/VideoModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L104)

<a id="Vercel_AI_Gateway_GatewayVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L64)

<a id="Vercel_AI_Gateway_GatewayVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L169)

<a id="Vercel_AI_Gateway_GatewayVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L214)

<a id="Vercel_AI_Gateway_GatewayVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L160)

<a id="Vercel_AI_Gateway_GatewayVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayVideoModel.cs#L58)
