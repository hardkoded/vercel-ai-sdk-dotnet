---
title: "GoogleProvider"
description: "Google Gemini provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleProvider"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleProvider"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public class GoogleProvider : ProviderBase, IBatchProvider
```


Google Gemini provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/)

**Derived:** [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L46)

## Constructors

<a id="Vercel_AI_Google_GoogleProvider__ctor_"></a>
<a id="Vercel_AI_Google_GoogleProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Google_GoogleOptions_"></a>

### GoogleProvider(HttpClient, GoogleOptions?)


```csharp
public GoogleProvider(HttpClient httpClient, GoogleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [GoogleOptions](/api/Vercel.AI.Google/GoogleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L52)

<a id="Vercel_AI_Google_GoogleProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Google_GoogleOptions_System_String_"></a>

### GoogleProvider(HttpClient, GoogleOptions?, string)


```csharp
protected GoogleProvider(HttpClient httpClient, GoogleOptions? options, string modelProvider)
```


Creates a provider with an explicit model provider id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [GoogleOptions](/api/Vercel.AI.Google/GoogleOptions/) |  |
| `modelProvider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L58)

## Fields

<a id="Vercel_AI_Google_GoogleProvider_ProviderName"></a>

### ProviderName


```csharp
public const string ProviderName = "google"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L49)

## Properties

<a id="Vercel_AI_Google_GoogleProvider_Clock_"></a>
<a id="Vercel_AI_Google_GoogleProvider_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset> Clock { get; set; }
```


Clock used for response timestamps.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L82)

<a id="Vercel_AI_Google_GoogleProvider_Http_"></a>
<a id="Vercel_AI_Google_GoogleProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L76)

<a id="Vercel_AI_Google_GoogleProvider_ModelProvider_"></a>
<a id="Vercel_AI_Google_GoogleProvider_ModelProvider"></a>

### ModelProvider


```csharp
public string ModelProvider { get; }
```


Provider id recorded on models, such as `google.generative-ai` or `google.vertex.chat`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L79)

<a id="Vercel_AI_Google_GoogleProvider_Options_"></a>
<a id="Vercel_AI_Google_GoogleProvider_Options"></a>

### Options


```csharp
public GoogleOptions Options { get; }
```


Options.

**Value:** [GoogleOptions](/api/Vercel.AI.Google/GoogleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L67)

## Methods

<a id="Vercel_AI_Google_GoogleProvider_Create_"></a>
<a id="Vercel_AI_Google_GoogleProvider_Create_Vercel_AI_Google_GoogleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(GoogleOptions?, HttpMessageHandler?)


```csharp
public static GoogleProvider Create(GoogleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GoogleOptions](/api/Vercel.AI.Google/GoogleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GoogleProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L85)

<a id="Vercel_AI_Google_GoogleProvider_DecisionModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public DecisionLanguageModel DecisionModel(string modelId)
```


Experimental Choice/Score/Boolean decision model that uses Gemini structured output.
The provider id is `google.decision`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionLanguageModel](/api/Vercel.AI.Operations/DecisionLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L101)

<a id="Vercel_AI_Google_GoogleProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public override IEmbeddingModel EmbeddingModel(string modelId)
```


Returns an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L120)

<a id="Vercel_AI_Google_GoogleProvider_ExperimentalBatch_"></a>
<a id="Vercel_AI_Google_GoogleProvider_ExperimentalBatch"></a>

### ExperimentalBatch()


```csharp
public IBatchApi? ExperimentalBatch()
```


Gemini Batch API. Downloads batch results.

**Returns:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L70)

<a id="Vercel_AI_Google_GoogleProvider_ImageModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public override IImageModel ImageModel(string modelId)
```


Returns an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L126)

<a id="Vercel_AI_Google_GoogleProvider_Interactions_"></a>
<a id="Vercel_AI_Google_GoogleProvider_Interactions_System_String_"></a>

### Interactions(string)


```csharp
public virtual ILanguageModel Interactions(string modelId)
```


Language model that posts to the Interactions API.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L108)

<a id="Vercel_AI_Google_GoogleProvider_Interactions_Vercel_AI_Google_GoogleInteractionsAgent_"></a>

### Interactions(GoogleInteractionsAgent)


```csharp
public virtual ILanguageModel Interactions(GoogleInteractionsAgent agent)
```


Language model that calls an agent through the Interactions API. The request sends `agent` instead of `model`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `agent` | [GoogleInteractionsAgent](/api/Vercel.AI.Google/GoogleInteractionsAgent/) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L114)

<a id="Vercel_AI_Google_GoogleProvider_LanguageModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L92)

<a id="Vercel_AI_Google_GoogleProvider_RealtimeModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_RealtimeModel_System_String_"></a>

### RealtimeModel(string)


```csharp
public override IRealtimeModel RealtimeModel(string modelId)
```


Returns a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L144)

<a id="Vercel_AI_Google_GoogleProvider_SpeechModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_SpeechModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L132)

<a id="Vercel_AI_Google_GoogleProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Google_GoogleProvider_TranscriptionModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L138)
