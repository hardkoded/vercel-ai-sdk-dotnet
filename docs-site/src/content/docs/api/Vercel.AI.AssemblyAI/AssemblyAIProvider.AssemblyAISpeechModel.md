---
title: "AssemblyAIProvider.AssemblyAISpeechModel"
description: "AssemblyAI text-to-speech model. Requests are buffered, not streamed."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIProvider.AssemblyAISpeechModel"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAIProvider.AssemblyAISpeechModel : ISpeechModel
```


AssemblyAI text-to-speech model. Requests are buffered, not streamed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L507)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel__ctor_Vercel_AI_AssemblyAI_AssemblyAIProvider_System_String_"></a>

### AssemblyAISpeechModel(AssemblyAIProvider, string)


```csharp
public AssemblyAISpeechModel(AssemblyAIProvider provider, string modelId)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AssemblyAIProvider](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L535)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_ModelId_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L545)

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_Provider_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L542)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechCallOptions, CancellationToken)


```csharp
public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
```


Synthesizes speech.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SpeechCallOptions](/api/Vercel.AI.Provider/SpeechCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L548)

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAISpeechModel_GenerateAsync_Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_System_Threading_CancellationToken_"></a>

### GenerateAsync(AssemblyAISpeechRequest, CancellationToken)


```csharp
public Task<AssemblyAISpeechResult> GenerateAsync(AssemblyAISpeechRequest request, CancellationToken cancellationToken)
```


Synthesizes speech and returns the audio with warnings, usage, and response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [AssemblyAISpeechRequest](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AssemblyAISpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L576)
