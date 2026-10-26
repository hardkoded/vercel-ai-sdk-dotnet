---
title: "Transcribe"
description: "Transcribes audio. Maps to transcribe."
editUrl: false
prev: false
next: false
sidebar:
  label: "Transcribe"
  hidden: true
---

<a id="Vercel_AI_Operations_Transcribe"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class Transcribe
```


Transcribes audio. Maps to `transcribe`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L555)

## Methods

<a id="Vercel_AI_Operations_Transcribe_TranscribeAsync_"></a>
<a id="Vercel_AI_Operations_Transcribe_TranscribeAsync_Vercel_AI_Operations_TranscribeRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(TranscribeRequest, CancellationToken)


```csharp
public static Task<TranscribeResult> TranscribeAsync(TranscribeRequest request, CancellationToken cancellationToken = default)
```


Transcribes one audio payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [TranscribeRequest](/api/Vercel.AI.Operations/TranscribeRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscribeResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L558)
