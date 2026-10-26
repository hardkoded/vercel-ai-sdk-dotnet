---
title: "StartVideo"
description: "Starts a video generation. Maps to experimentalstartVideo."
editUrl: false
prev: false
next: false
sidebar:
  label: "StartVideo"
  hidden: true
---

<a id="Vercel_AI_Operations_StartVideo"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class StartVideo
```


Starts a video generation. Maps to `experimental_startVideo`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L999)

## Methods

<a id="Vercel_AI_Operations_StartVideo_StartVideoAsync_"></a>
<a id="Vercel_AI_Operations_StartVideo_StartVideoAsync_Vercel_AI_Operations_GenerateVideoRequest_System_Threading_CancellationToken_"></a>

### StartVideoAsync(GenerateVideoRequest, CancellationToken)


```csharp
public static Task<StartVideoResult> StartVideoAsync(GenerateVideoRequest request, CancellationToken cancellationToken = default)
```


Starts one operation and returns without waiting.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [GenerateVideoRequest](/api/Vercel.AI.Operations/GenerateVideoRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;StartVideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L1002)
