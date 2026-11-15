---
title: "MiniMaxVideoModel"
description: "MiniMax video model. This is the shell the provider factories create. Request building, polling, and errors are not ported yet, so every operation throws Not..."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxVideoModel"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxVideoModel : IVideoModel, IVideoCaller
```


MiniMax video model. This is the shell the provider factories create. Request building, polling, and errors are not ported yet,
so every operation throws [NotSupportedException](https://learn.microsoft.com/dotnet/api/system.notsupportedexception).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/), [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L39)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel__ctor_System_String_Vercel_AI_MiniMax_MiniMaxVideoModelConfig_"></a>

### MiniMaxVideoModel(string, MiniMaxVideoModelConfig)


```csharp
public MiniMaxVideoModel(string modelId, MiniMaxVideoModelConfig config)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [MiniMaxVideoModelConfig](/api/Vercel.AI.MiniMax/MiniMaxVideoModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L42)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanGenerate_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanGenerate"></a>

### CanGenerate


```csharp
public bool CanGenerate { get; }
```


True when `doGenerate` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L61)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanStart_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanStart"></a>

### CanStart


```csharp
public bool CanStart { get; }
```


True when `doStart` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L64)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanStatus_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanStatus"></a>

### CanStatus


```csharp
public bool CanStatus { get; }
```


True when `doStatus` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L67)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanWebhook_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_CanWebhook"></a>

### CanWebhook


```csharp
public bool CanWebhook { get; }
```


True when `handleWebhookOption` exists.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L70)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_Config_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_Config"></a>

### Config


```csharp
public MiniMaxVideoModelConfig Config { get; }
```


The configuration the provider passed in.

**Value:** [MiniMaxVideoModelConfig](/api/Vercel.AI.MiniMax/MiniMaxVideoModelConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L73)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_MaxVideosPerCall_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; }
```


MiniMax creates one video per task.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L58)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_ModelId_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L55)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_Provider_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L52)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_SpecificationVersion_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L49)

## Methods

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoGenerateAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L88)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoGenerateAsync_Vercel_AI_Provider_VideoCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L82)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoStartAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoStartAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L94)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoStatusAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_DoStatusAsync_Vercel_AI_Operations_VideoModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L100)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_HandleWebhookAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_HandleWebhookAsync_System_Func_System_Threading_Tasks_Task_Vercel_AI_Operations_VideoWebhook___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L106)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_ResolveMaxVideosPerCallAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModel_ResolveMaxVideosPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L76)
