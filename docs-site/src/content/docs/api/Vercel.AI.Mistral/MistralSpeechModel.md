---
title: "MistralSpeechModel"
description: "Mistral text-to-speech model. Maps to MistralSpeechModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralSpeechModel"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralSpeechModel : ISpeechModel, ISpeechCaller
```


Mistral text-to-speech model. Maps to `MistralSpeechModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/), [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L16)

## Constructors

<a id="Vercel_AI_Mistral_MistralSpeechModel__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_System_Func_System_DateTime__"></a>

### `MistralSpeechModel(OpenAICompatibleProvider, string, Func<DateTime>?)`


```csharp
public MistralSpeechModel(OpenAICompatibleProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a speech model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now. Maps to `_internal.currentDate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L27)

## Properties

<a id="Vercel_AI_Mistral_MistralSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L45)

<a id="Vercel_AI_Mistral_MistralSpeechModel_Provider_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L42)

<a id="Vercel_AI_Mistral_MistralSpeechModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L39)

## Methods

<a id="Vercel_AI_Mistral_MistralSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModel_DoGenerateAsync_Vercel_AI_Operations_SpeechModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechModelCall, CancellationToken)


```csharp
public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
```


Generates speech audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [SpeechModelCall](/api/Vercel.AI.Operations/SpeechModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L84)

<a id="Vercel_AI_Mistral_MistralSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModel.cs#L48)
