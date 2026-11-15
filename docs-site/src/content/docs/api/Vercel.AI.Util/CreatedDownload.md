---
title: "CreatedDownload"
description: "Download function created by DownloadFetch). C# has no callable overloads, so the two upstream call signatures are DownloadRequestItem%7d) and InvokeAsync(Do..."
editUrl: false
prev: false
next: false
sidebar:
  label: "CreatedDownload"
  hidden: true
---

<a id="Vercel_AI_Util_CreatedDownload"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class CreatedDownload
```


Download function created by `DownloadFetch)`. C# has no callable overloads, so the two upstream call
signatures are `DownloadRequestItem%7d)` and [InvokeAsync(DownloadRequestItem)](#Vercel_AI_Util_CreatedDownload_InvokeAsync_Vercel_AI_Util_DownloadRequestItem_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L193)

## Methods

<a id="Vercel_AI_Util_CreatedDownload_InvokeAsync_"></a>
<a id="Vercel_AI_Util_CreatedDownload_InvokeAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadRequestItem__"></a>

### `InvokeAsync(IReadOnlyList<DownloadRequestItem>)`


```csharp
public Task<IReadOnlyList<DownloadResult>> InvokeAsync(IReadOnlyList<DownloadRequestItem> requests)
```


Downloads a batch. A URL the model supports is passed through as a null result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `requests` | [IReadOnlyList&lt;DownloadRequestItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L216)

<a id="Vercel_AI_Util_CreatedDownload_InvokeAsync_Vercel_AI_Util_DownloadRequestItem_"></a>

### InvokeAsync(DownloadRequestItem)


```csharp
public Task<DownloadResult> InvokeAsync(DownloadRequestItem request)
```


Downloads one request. This is the shape of the `Download` option of transcribe and generateVideo.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [DownloadRequestItem](/api/Vercel.AI.Util/DownloadRequestItem/) |  |

**Returns:** [Task&lt;DownloadResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L205)

## Operators

<a id="Vercel_AI_Util_CreatedDownload_op_Implicit_"></a>
<a id="Vercel_AI_Util_CreatedDownload_op_Implicit_Vercel_AI_Util_CreatedDownload__System_Func_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadRequestItem__System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadResult___"></a>

### `implicit operator Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>(CreatedDownload)`


```csharp
public static implicit operator Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>(CreatedDownload download)
```


Converts to the batch shape of `ExperimentalDownload`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `download` | [CreatedDownload](./) |  |

**Returns:** [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L234)

<a id="Vercel_AI_Util_CreatedDownload_op_Implicit_Vercel_AI_Util_CreatedDownload__System_Func_System_String_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Operations_DownloadedMedia__"></a>

### `implicit operator Func<string, CancellationToken, Task<DownloadedMedia>>(CreatedDownload)`


```csharp
public static implicit operator Func<string, CancellationToken, Task<DownloadedMedia>>(CreatedDownload download)
```


Converts to the single-request shape of the `Download` option of transcribe and generateVideo.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `download` | [CreatedDownload](./) |  |

**Returns:** [Func&lt;string, CancellationToken, Task&lt;DownloadedMedia&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L240)
