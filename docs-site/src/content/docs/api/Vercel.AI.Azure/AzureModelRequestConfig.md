---
title: "AzureModelRequestConfig"
description: "Request routing of an Azure-owned model. Maps to AzureRequestConfig in azure-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureModelRequestConfig"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureModelRequestConfig"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureModelRequestConfig
```


Request routing of an Azure-owned model. Maps to `AzureRequestConfig` in `azure-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L8)

## Properties

<a id="Vercel_AI_Azure_AzureModelRequestConfig_ImageProviderOptions_"></a>
<a id="Vercel_AI_Azure_AzureModelRequestConfig_ImageProviderOptions"></a>

### ImageProviderOptions


```csharp
public IReadOnlyDictionary<string, bool>? ImageProviderOptions { get; }
```


Image provider options the model supports, by name. Null for models without an image entry. Maps to `providerOptions.image`.

**Value:** [IReadOnlyDictionary&lt;string, bool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L28)

<a id="Vercel_AI_Azure_AzureModelRequestConfig_MaiImage_"></a>
<a id="Vercel_AI_Azure_AzureModelRequestConfig_MaiImage"></a>

### MaiImage


```csharp
public bool MaiImage { get; }
```


True when the model uses the MAI image API. Maps to `image.api === 'mai'`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L31)

<a id="Vercel_AI_Azure_AzureModelRequestConfig_MaiStreamingTranscription_"></a>
<a id="Vercel_AI_Azure_AzureModelRequestConfig_MaiStreamingTranscription"></a>

### MaiStreamingTranscription


```csharp
public bool MaiStreamingTranscription { get; }
```


True when the model uses MAI streaming transcription. Maps to `transcription.api === 'mai'`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L43)

<a id="Vercel_AI_Azure_AzureModelRequestConfig_SpeechVoiceModel_"></a>
<a id="Vercel_AI_Azure_AzureModelRequestConfig_SpeechVoiceModel"></a>

### SpeechVoiceModel


```csharp
public string? SpeechVoiceModel { get; }
```


Voice model name for the Azure Speech API, for example `MAI-Voice-2`. Maps to `speech.voiceModel`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L34)

<a id="Vercel_AI_Azure_AzureModelRequestConfig_TranscribeName_"></a>
<a id="Vercel_AI_Azure_AzureModelRequestConfig_TranscribeName"></a>

### TranscribeName


```csharp
public string? TranscribeName { get; }
```


Model name for the Azure Speech transcription API. Maps to `transcription.name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L37)

<a id="Vercel_AI_Azure_AzureModelRequestConfig_TranscribeSupportsTimestamps_"></a>
<a id="Vercel_AI_Azure_AzureModelRequestConfig_TranscribeSupportsTimestamps"></a>

### TranscribeSupportsTimestamps


```csharp
public bool TranscribeSupportsTimestamps { get; }
```


True when the Speech transcription model returns timestamps. Maps to `transcription.supportsTimestamps`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureModelCatalog.cs#L40)
