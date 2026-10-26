---
title: "GenerateSpeech"
description: "Generates speech. Maps to generateSpeech."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeech"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateSpeech"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class GenerateSpeech
```


Generates speech. Maps to `generateSpeech`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L175)

## Methods

<a id="Vercel_AI_Operations_GenerateSpeech_GenerateSpeechAsync_"></a>
<a id="Vercel_AI_Operations_GenerateSpeech_GenerateSpeechAsync_Vercel_AI_Operations_GenerateSpeechRequest_System_Threading_CancellationToken_"></a>

### GenerateSpeechAsync(GenerateSpeechRequest, CancellationToken)


```csharp
public static Task<GenerateSpeechResult> GenerateSpeechAsync(GenerateSpeechRequest request, CancellationToken cancellationToken = default)
```


Generates speech audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [GenerateSpeechRequest](/api/Vercel.AI.Operations/GenerateSpeechRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateSpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L178)
