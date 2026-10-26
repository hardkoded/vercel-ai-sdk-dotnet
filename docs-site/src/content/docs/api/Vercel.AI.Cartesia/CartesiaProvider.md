---
title: "CartesiaProvider"
description: "Cartesia provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaProvider"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaProvider"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public sealed class CartesiaProvider : OpenAICompatibleProvider
```


Cartesia provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L18)

## Constructors

<a id="Vercel_AI_Cartesia_CartesiaProvider__ctor_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### CartesiaProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public CartesiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L29)

## Fields

<a id="Vercel_AI_Cartesia_CartesiaProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.cartesia.ai"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L24)

<a id="Vercel_AI_Cartesia_CartesiaProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "cartesia"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L21)

## Properties

<a id="Vercel_AI_Cartesia_CartesiaProvider_ErrorSchema_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider_ErrorSchema"></a>

### ErrorSchema


```csharp
public static JsonObject ErrorSchema { get; }
```


Schema of a Cartesia error body.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L36)

## Methods

<a id="Vercel_AI_Cartesia_CartesiaProvider_Create_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static CartesiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [CartesiaProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L57)

<a id="Vercel_AI_Cartesia_CartesiaProvider_GenerateSpeechAsync_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider_GenerateSpeechAsync_System_String_Vercel_AI_Provider_SpeechCallOptions_System_Collections_Generic_IDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `GenerateSpeechAsync(string, SpeechCallOptions, IDictionary<string, string>?, CancellationToken)`


```csharp
public Task<SpeechResult> GenerateSpeechAsync(string modelId, SpeechCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
```


Synthesizes speech. &lt;code class="paramref"&gt;headers&lt;/code&gt; override the provider headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [SpeechCallOptions](/api/Vercel.AI.Provider/SpeechCallOptions/) |  |
| `headers` | [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L84)

<a id="Vercel_AI_Cartesia_CartesiaProvider_LanguageModel_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L51)

<a id="Vercel_AI_Cartesia_CartesiaProvider_SpeechModel_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider_SpeechModel_System_String_"></a>

### SpeechModel(string)


```csharp
public override ISpeechModel SpeechModel(string modelId)
```


Returns a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L78)

<a id="Vercel_AI_Cartesia_CartesiaProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Cartesia_CartesiaProvider_TranscriptionModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L81)
