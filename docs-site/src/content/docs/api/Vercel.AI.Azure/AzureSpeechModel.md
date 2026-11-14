---
title: "AzureSpeechModel"
description: "Azure speech model. Resolves the API for every call: providerOptions.azure.api wins, and otherwise MAI-Voice models use the Azure Speech API and every other..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureSpeechModel"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureSpeechModel : ISpeechModel
```


Azure speech model. Resolves the API for every call: `providerOptions.azure.api` wins, and otherwise
MAI-Voice models use the Azure Speech API and every other model uses the OpenAI speech API. Maps to
`AzureSpeechModel` in `azure-openai-provider.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L16)

## Properties

<a id="Vercel_AI_Azure_AzureSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L32)

<a id="Vercel_AI_Azure_AzureSpeechModel_Provider_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L29)

## Methods

<a id="Vercel_AI_Azure_AzureSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L35)

<a id="Vercel_AI_Azure_AzureSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_GenerateAsync_Vercel_AI_OpenAI_OpenAISpeechCall_System_Threading_CancellationToken_"></a>

### GenerateAsync(OpenAISpeechCall, CancellationToken)


```csharp
public Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, CancellationToken cancellationToken)
```


Synthesizes speech with the OpenAI or the Azure Speech API and returns the audio, warnings and response metadata.
Maps to `doGenerate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAISpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | An `azure` provider option is unknown or out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L51)
