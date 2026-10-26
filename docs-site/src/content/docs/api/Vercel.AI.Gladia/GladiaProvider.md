---
title: "GladiaProvider"
description: "Gladia pre-recorded transcription provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaProvider"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaProvider"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaProvider : OpenAICompatibleProvider
```


Gladia pre-recorded transcription provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L175)

## Constructors

<a id="Vercel_AI_Gladia_GladiaProvider__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### GladiaProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public GladiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L186)

## Fields

<a id="Vercel_AI_Gladia_GladiaProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.gladia.io"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L181)

<a id="Vercel_AI_Gladia_GladiaProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "gladia"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L178)

## Methods

<a id="Vercel_AI_Gladia_GladiaProvider_Create_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static GladiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GladiaProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L193)

<a id="Vercel_AI_Gladia_GladiaProvider_LanguageModel_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Returns a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L200)

<a id="Vercel_AI_Gladia_GladiaProvider_Transcription_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_Transcription"></a>

### Transcription()


```csharp
public GladiaProvider.GladiaTranscriptionModel Transcription()
```


Creates the pre-recorded transcription model.

**Returns:** [GladiaProvider.GladiaTranscriptionModel](/api/Vercel.AI.Gladia/GladiaProvider.GladiaTranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L206)

<a id="Vercel_AI_Gladia_GladiaProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public override ITranscriptionModel TranscriptionModel(string modelId)
```


Returns a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L212)
