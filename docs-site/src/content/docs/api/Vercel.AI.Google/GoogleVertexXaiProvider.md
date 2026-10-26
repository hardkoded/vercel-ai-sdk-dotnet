---
title: "GoogleVertexXaiProvider"
description: "xAI Grok models hosted on Vertex."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexXaiProvider"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexXaiProvider"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexXaiProvider
```


xAI Grok models hosted on Vertex.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L312)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexXaiProvider__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider__ctor_System_String_System_String_System_String_System_Net_Http_HttpMessageHandler_"></a>

### GoogleVertexXaiProvider(string, string?, string?, HttpMessageHandler?)


```csharp
public GoogleVertexXaiProvider(string project, string? location, string? baseUrl = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L317)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider__ctor_Vercel_AI_Google_GoogleVertexPartnerOptions_System_Net_Http_HttpMessageHandler_"></a>

### GoogleVertexXaiProvider(GoogleVertexPartnerOptions, HttpMessageHandler?)


```csharp
public GoogleVertexXaiProvider(GoogleVertexPartnerOptions options, HttpMessageHandler? handler = null)
```


Creates a provider. &lt;code class="paramref"&gt;handler&lt;/code&gt; sends the request after the auth headers are added.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GoogleVertexPartnerOptions](/api/Vercel.AI.Google/GoogleVertexPartnerOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L323)

## Properties

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


OpenAI-compatible base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L339)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_ClientCreated_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_ClientCreated"></a>

### ClientCreated


```csharp
public bool ClientCreated { get; }
```


True after the inner provider has been created.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L342)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_Location_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_Location"></a>

### Location


```csharp
public string Location { get; }
```


Location.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L336)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_Project_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_Project"></a>

### Project


```csharp
public string Project { get; }
```


Project id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L333)

## Methods

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_ConvertUsage_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_ConvertUsage_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### ConvertUsage(int?, int?, int?, int?)


```csharp
public static GoogleTokenUsage ConvertUsage(int? promptTokens, int? completionTokens, int? cachedTokens, int? reasoningTokens)
```


Counts Grok reasoning tokens separately. Output total is completion tokens plus reasoning tokens.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `promptTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `completionTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cachedTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `reasoningTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [GoogleTokenUsage](/api/Vercel.AI.Google/GoogleTokenUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L382)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public IEmbeddingModel EmbeddingModel(string modelId)
```


Embedding and image models are not available.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L392)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_LanguageModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public ILanguageModel LanguageModel(string modelId)
```


Returns a chat model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L351)

<a id="Vercel_AI_Google_GoogleVertexXaiProvider_TransformBody_"></a>
<a id="Vercel_AI_Google_GoogleVertexXaiProvider_TransformBody_System_Text_Json_Nodes_JsonObject_"></a>

### TransformBody(JsonObject)


```csharp
public static JsonObject TransformBody(JsonObject body)
```


Removes `reasoning_effort`. Vertex Grok does not accept it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L372)
