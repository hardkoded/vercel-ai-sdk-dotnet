---
title: "GetVideoStatus"
description: "Reads video operation status. Maps to experimentalgetVideoStatus."
editUrl: false
prev: false
next: false
sidebar:
  label: "GetVideoStatus"
  hidden: true
---

<a id="Vercel_AI_Operations_GetVideoStatus"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class GetVideoStatus
```


Reads video operation status. Maps to `experimental_getVideoStatus`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L1038)

## Methods

<a id="Vercel_AI_Operations_GetVideoStatus_GetVideoStatusAsync_"></a>
<a id="Vercel_AI_Operations_GetVideoStatus_GetVideoStatusAsync_Vercel_AI_Operations_GetVideoStatusRequest_System_Threading_CancellationToken_"></a>

### GetVideoStatusAsync(GetVideoStatusRequest, CancellationToken)


```csharp
public static Task<VideoStatusResult> GetVideoStatusAsync(GetVideoStatusRequest request, CancellationToken cancellationToken = default)
```


Performs one status check.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [GetVideoStatusRequest](/api/Vercel.AI.Operations/GetVideoStatusRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoStatusResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L1041)
