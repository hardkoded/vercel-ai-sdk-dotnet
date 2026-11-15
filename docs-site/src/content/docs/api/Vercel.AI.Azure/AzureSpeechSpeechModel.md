---
title: "AzureSpeechSpeechModel"
description: "Azure Speech text to speech with SSML. The model is appended to the voice name, for example en-US-Harper:MAI-Voice-2. Maps to AzureSpeechSpeechModel in azure..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureSpeechSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureSpeechSpeechModel"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureSpeechSpeechModel : ISpeechModel
```


Azure Speech text to speech with SSML. The model is appended to the voice name, for example
`en-US-Harper:MAI-Voice-2`. Maps to `AzureSpeechSpeechModel` in `azure-speech-speech-model.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModel.cs#L20)

## Constructors

<a id="Vercel_AI_Azure_AzureSpeechSpeechModel__ctor_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModel__ctor_System_String_Vercel_AI_ProviderUtils_ProviderHttp_System_Func_System_Uri__System_Func_System_Collections_Generic_Dictionary_System_String_System_String___System_Func_System_DateTimeOffset__"></a>

### `AzureSpeechSpeechModel(string, ProviderHttp, Func<Uri>, Func<Dictionary<string, string?>>, Func<DateTimeOffset>?)`


```csharp
public AzureSpeechSpeechModel(string modelId, ProviderHttp http, Func<Uri> url, Func<Dictionary<string, string?>> headers, Func<DateTimeOffset>? currentDate = null)
```


Creates an Azure Speech model. &lt;code class="paramref"&gt;url&lt;/code&gt; and &lt;code class="paramref"&gt;headers&lt;/code&gt; are read on every call.
&lt;code class="paramref"&gt;currentDate&lt;/code&gt; sets the response timestamp. Maps to the config of `AzureSpeechSpeechModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `http` | [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/) |  |
| `url` | [Func&lt;Uri&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `headers` | [Func&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `currentDate` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModel.cs#L73)

## Properties

<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModel.cs#L91)

<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_Provider_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModel.cs#L88)

## Methods

<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModel.cs#L94)

<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModel_GenerateAsync_Vercel_AI_OpenAI_OpenAISpeechCall_Vercel_AI_Azure_AzureSpeechSpeechModelOptions_System_Threading_CancellationToken_"></a>

### GenerateAsync(OpenAISpeechCall, AzureSpeechSpeechModelOptions?, CancellationToken)


```csharp
public Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, AzureSpeechSpeechModelOptions? azureOptions, CancellationToken cancellationToken)
```


Synthesizes speech with SSML. Maps to `doGenerate(options, azureOptions)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) |  |
| `azureOptions` | [AzureSpeechSpeechModelOptions](/api/Vercel.AI.Azure/AzureSpeechSpeechModelOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAISpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModel.cs#L106)
