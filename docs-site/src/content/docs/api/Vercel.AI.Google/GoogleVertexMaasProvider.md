---
title: "GoogleVertexMaasProvider"
description: "Vertex MaaS OpenAI-compatible endpoint."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexMaasProvider"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexMaasProvider"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexMaasProvider
```


Vertex MaaS OpenAI-compatible endpoint.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L236)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexMaasProvider__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider__ctor_System_String_System_String_System_String_System_Net_Http_HttpMessageHandler_"></a>

### GoogleVertexMaasProvider(string, string?, string?, HttpMessageHandler?)


```csharp
public GoogleVertexMaasProvider(string project, string? location, string? baseUrl = null, HttpMessageHandler? handler = null)
```


Creates a provider. The OpenAI-compatible client is created on first use.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `project` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `location` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L247)

<a id="Vercel_AI_Google_GoogleVertexMaasProvider__ctor_Vercel_AI_Google_GoogleVertexPartnerOptions_System_Net_Http_HttpMessageHandler_"></a>

### GoogleVertexMaasProvider(GoogleVertexPartnerOptions, HttpMessageHandler?)


```csharp
public GoogleVertexMaasProvider(GoogleVertexPartnerOptions options, HttpMessageHandler? handler = null)
```


Creates a provider. &lt;code class="paramref"&gt;handler&lt;/code&gt; sends the request after the auth headers are added.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GoogleVertexPartnerOptions](/api/Vercel.AI.Google/GoogleVertexPartnerOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L253)

## Properties

<a id="Vercel_AI_Google_GoogleVertexMaasProvider_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


OpenAI-compatible base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L269)

<a id="Vercel_AI_Google_GoogleVertexMaasProvider_ClientCreated_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider_ClientCreated"></a>

### ClientCreated


```csharp
public bool ClientCreated { get; }
```


True after [LanguageModel(string)](#Vercel_AI_Google_GoogleVertexMaasProvider_LanguageModel_System_String_) has created the inner provider.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L272)

<a id="Vercel_AI_Google_GoogleVertexMaasProvider_Location_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider_Location"></a>

### Location


```csharp
public string Location { get; }
```


Location. Defaults to `global`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L266)

<a id="Vercel_AI_Google_GoogleVertexMaasProvider_Project_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider_Project"></a>

### Project


```csharp
public string Project { get; }
```


Project id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L263)

## Methods

<a id="Vercel_AI_Google_GoogleVertexMaasProvider_LanguageModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public ILanguageModel LanguageModel(string modelId)
```


Returns a chat model and caches the inner provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L281)

<a id="Vercel_AI_Google_GoogleVertexMaasProvider_TransformBody_"></a>
<a id="Vercel_AI_Google_GoogleVertexMaasProvider_TransformBody_System_String_System_Text_Json_Nodes_JsonObject_"></a>

### TransformBody(string, JsonObject)


```csharp
public static JsonObject TransformBody(string modelId, JsonObject body)
```


Sets Llama 4 `max_tokens` when the caller did not.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L299)
