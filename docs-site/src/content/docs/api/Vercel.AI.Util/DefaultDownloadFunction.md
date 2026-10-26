---
title: "DefaultDownloadFunction"
description: "Default download function: downloads URLs the model cannot fetch itself."
editUrl: false
prev: false
next: false
sidebar:
  label: "DefaultDownloadFunction"
  hidden: true
---

<a id="Vercel_AI_Util_DefaultDownloadFunction"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class DefaultDownloadFunction
```


Default download function: downloads URLs the model cannot fetch itself.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L798)

## Methods

<a id="Vercel_AI_Util_DefaultDownloadFunction_Create_"></a>
<a id="Vercel_AI_Util_DefaultDownloadFunction_Create_System_Func_Vercel_AI_Util_AssetDownloadCall_System_Threading_Tasks_Task_Vercel_AI_Util_DownloadResult___System_Threading_CancellationToken_"></a>

### `Create(Func<AssetDownloadCall, Task<DownloadResult>>?, CancellationToken)`


```csharp
public static Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> Create(Func<AssetDownloadCall, Task<DownloadResult>>? download = null, CancellationToken abortSignal = default)
```


Creates a function that downloads each URL the model does not support.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `download` | [Func&lt;AssetDownloadCall, Task&lt;DownloadResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L801)
