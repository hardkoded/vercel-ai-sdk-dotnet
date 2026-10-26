---
title: "OpenAICompatibleProvider"
description: "Provider that speaks the OpenAI Chat Completions, embeddings, and images APIs."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleProvider"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public class OpenAICompatibleProvider : ProviderBase
```


Provider that speaks the OpenAI Chat Completions, embeddings, and images APIs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

**Derived:** [AlibabaProvider](/api/Vercel.AI.Alibaba/AlibabaProvider/), [AssemblyAIProvider](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider/), [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/), [BasetenProvider](/api/Vercel.AI.Baseten/BasetenProvider/), [BlackForestLabsProvider](/api/Vercel.AI.BlackForestLabs/BlackForestLabsProvider/), [ByteDanceProvider](/api/Vercel.AI.ByteDance/ByteDanceProvider/), [CartesiaProvider](/api/Vercel.AI.Cartesia/CartesiaProvider/), [CerebrasProvider](/api/Vercel.AI.Cerebras/CerebrasProvider/), [DeepInfraProvider](/api/Vercel.AI.DeepInfra/DeepInfraProvider/), [DeepSeekProvider](/api/Vercel.AI.DeepSeek/DeepSeekProvider/), [DeepgramProvider](/api/Vercel.AI.Deepgram/DeepgramProvider/), [ElevenLabsProvider](/api/Vercel.AI.ElevenLabs/ElevenLabsProvider/), [FalProvider](/api/Vercel.AI.Fal/FalProvider/), [FireworksProvider](/api/Vercel.AI.Fireworks/FireworksProvider/), [FishAudioProvider](/api/Vercel.AI.FishAudio/FishAudioProvider/), [GladiaProvider](/api/Vercel.AI.Gladia/GladiaProvider/), [GmiCloudProvider](/api/Vercel.AI.GmiCloud/GmiCloudProvider/), [GroqProvider](/api/Vercel.AI.Groq/GroqProvider/), [HeyGenProvider](/api/Vercel.AI.HeyGen/HeyGenProvider/), [HuggingFaceProvider](/api/Vercel.AI.HuggingFace/HuggingFaceProvider/), [HumeProvider](/api/Vercel.AI.Hume/HumeProvider/), [KlingAIProvider](/api/Vercel.AI.KlingAI/KlingAIProvider/), [LumaProvider](/api/Vercel.AI.Luma/LumaProvider/), [MistralProvider](/api/Vercel.AI.Mistral/MistralProvider/), [MoonshotProvider](/api/Vercel.AI.Moonshot/MoonshotProvider/), [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/), [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/), [ProdiaProvider](/api/Vercel.AI.Prodia/ProdiaProvider/), [QuiverAIProvider](/api/Vercel.AI.QuiverAI/QuiverAIProvider/), [ReplicateProvider](/api/Vercel.AI.Replicate/ReplicateProvider/), [RevAIProvider](/api/Vercel.AI.RevAI/RevAIProvider/), [TogetherAIProvider](/api/Vercel.AI.TogetherAI/TogetherAIProvider/), [VoyageProvider](/api/Vercel.AI.Voyage/VoyageProvider/), [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/), [ZaiProvider](/api/Vercel.AI.Zai/ZaiProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L17)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpClient_"></a>

### OpenAICompatibleProvider(OpenAICompatibleOptions, HttpClient)


```csharp
public OpenAICompatibleProvider(OpenAICompatibleOptions options, HttpClient httpClient)
```


Creates a provider. The caller owns &lt;code class="paramref"&gt;httpClient&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L22)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_Http_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper used by the models.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L34)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_Options_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_Options"></a>

### Options


```csharp
public OpenAICompatibleOptions Options { get; }
```


Options used to build requests.

**Value:** [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L31)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ChatUri_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ChatUri_System_String_"></a>

### ChatUri(string)


```csharp
public Uri ChatUri(string modelId)
```


Chat Completions URL for a model. Azure deployment routes include the model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L406)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CompletionModel_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CompletionModel_System_String_"></a>

### CompletionModel(string)


```csharp
public virtual OpenAICompatibleCompletionLanguageModel CompletionModel(string modelId)
```


Creates a legacy Completions model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAICompatibleCompletionLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleCompletionLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L51)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CompletionsUri_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CompletionsUri_System_String_"></a>

### CompletionsUri(string)


```csharp
public Uri CompletionsUri(string modelId)
```


Completions URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L417)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_Create_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions, HttpMessageHandler?)


```csharp
public static OpenAICompatibleProvider Create(OpenAICompatibleOptions options, HttpMessageHandler? handler = null)
```


Creates a provider with an [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) that uses &lt;code class="paramref"&gt;handler&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [OpenAICompatibleProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L79)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CreateChatModel_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CreateChatModel_System_String_"></a>

### CreateChatModel(string)


```csharp
public virtual OpenAICompatibleLanguageModel CreateChatModel(string modelId)
```


Creates a chat model. Providers override this when support differs by model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAICompatibleLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L45)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CreateHeaders_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CreateHeaders"></a>

### CreateHeaders()


```csharp
public Dictionary<string, string?> CreateHeaders()
```


Resolves the API key and builds request headers.

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L86)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_CreateHeaders_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `CreateHeaders(IReadOnlyDictionary<string, string?>?)`


```csharp
public Dictionary<string, string?> CreateHeaders(IReadOnlyDictionary<string, string?>? callHeaders)
```


Builds headers and lets &lt;code class="paramref"&gt;callHeaders&lt;/code&gt; override them.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L92)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L57)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_EmbeddingsUri_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_EmbeddingsUri_System_String_"></a>

### EmbeddingsUri(string)


```csharp
public Uri EmbeddingsUri(string modelId)
```


Embeddings URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L428)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_GetBinaryAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_GetBinaryAsync_System_Uri_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `GetBinaryAsync(Uri, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<OpenAICompatibleBinaryResponse> GetBinaryAsync(Uri uri, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Downloads raw bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAICompatibleBinaryResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L273)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImageModel_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L68)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImagesEditsUri_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImagesEditsUri"></a>

### ImagesEditsUri()


```csharp
public Uri ImagesEditsUri()
```


Image edits URL.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L446)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImagesUri_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImagesUri"></a>

### ImagesUri()


```csharp
public Uri ImagesUri()
```


Images URL.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L439)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_LanguageModel_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L39)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostBinaryAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostBinaryAsync_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `PostBinaryAsync(Uri, string, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<OpenAICompatibleBinaryResponse> PostBinaryAsync(Uri uri, string json, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and returns the raw response bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAICompatibleBinaryResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L284)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostJsonAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostJsonAsync_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `PostJsonAsync(Uri, string, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<ProviderTextResponse> PostJsonAsync(Uri uri, string json, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and rewrites the provider error message when one is available.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ProviderTextResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L187)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostMultipartAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostMultipartAsync_System_Uri_System_Net_Http_MultipartFormDataContent_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `PostMultipartAsync(Uri, MultipartFormDataContent, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<ProviderTextResponse> PostMultipartAsync(Uri uri, MultipartFormDataContent content, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts multipart form data and rewrites the provider error message when one is available.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `content` | [MultipartFormDataContent](https://learn.microsoft.com/dotnet/api/system.net.http.multipartformdatacontent) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ProviderTextResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L261)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostSseAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_PostSseAsync_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `PostSseAsync(Uri, string, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<OpenAICompatibleSseResponse> PostSseAsync(Uri uri, string json, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and opens an SSE response. The caller disposes the result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAICompatibleSseResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L210)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_RerankUri_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_RerankUri"></a>

### RerankUri()


```csharp
public Uri RerankUri()
```


Rerank URL.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L453)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ResolveApiKey_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ResolveApiKey"></a>

### ResolveApiKey()


```csharp
protected virtual string? ResolveApiKey()
```


Resolves the API key. A missing key is omitted unless [RequireApiKey](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_RequireApiKey) is set.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L152)
