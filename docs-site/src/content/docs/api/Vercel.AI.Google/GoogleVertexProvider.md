---
title: "GoogleVertexProvider"
description: "Google Vertex AI provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexProvider"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexProvider"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexProvider : GoogleProvider, IBatchProvider
```


Google Vertex AI provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L240)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexProvider__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Google_VertexOptions_"></a>

### GoogleVertexProvider(HttpClient, VertexOptions?)


```csharp
public GoogleVertexProvider(HttpClient httpClient, VertexOptions? options = null)
```


Creates a Vertex provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [VertexOptions](/api/Vercel.AI.Google/VertexOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L243)

## Properties

<a id="Vercel_AI_Google_GoogleVertexProvider_Vertex_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider_Vertex"></a>

### Vertex


```csharp
public VertexOptions Vertex { get; }
```


Vertex options.

**Value:** [VertexOptions](/api/Vercel.AI.Google/VertexOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L250)

## Methods

<a id="Vercel_AI_Google_GoogleVertexProvider_Create_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider_Create_Vercel_AI_Google_VertexOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(VertexOptions?, HttpMessageHandler?)


```csharp
public static GoogleVertexProvider Create(VertexOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [VertexOptions](/api/Vercel.AI.Google/VertexOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GoogleVertexProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L253)

<a id="Vercel_AI_Google_GoogleVertexProvider_Interactions_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider_Interactions_System_String_"></a>

### Interactions(string)


```csharp
public override ILanguageModel Interactions(string modelId)
```


Interactions use the location resource and reject Express Mode.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L271)

<a id="Vercel_AI_Google_GoogleVertexProvider_Interactions_Vercel_AI_Google_GoogleInteractionsAgent_"></a>

### Interactions(GoogleInteractionsAgent)


```csharp
public override ILanguageModel Interactions(GoogleInteractionsAgent agent)
```


Interactions use the location resource and reject Express Mode.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `agent` | [GoogleInteractionsAgent](/api/Vercel.AI.Google/GoogleInteractionsAgent/) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L282)

<a id="Vercel_AI_Google_GoogleVertexProvider_LanguageModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Tuned `endpoints/` models reject Express Mode and omit `/publishers/google`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L260)

<a id="Vercel_AI_Google_GoogleVertexProvider_SpeechModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider_SpeechModel_System_String_"></a>

### SpeechModel(string)


```csharp
public override ISpeechModel SpeechModel(string modelId)
```


Chirp speech models use Cloud Text-to-Speech. Gemini speech models use generateContent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L293)

<a id="Vercel_AI_Google_GoogleVertexProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexProvider_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public override ITranscriptionModel TranscriptionModel(string modelId)
```


Gemini transcription ids use generateContent. Other ids use Cloud Speech-to-Text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L309)
