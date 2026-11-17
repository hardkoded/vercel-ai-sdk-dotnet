---
title: "GoogleVertexAnthropicProvider"
description: "Anthropic on Vertex. Sends the Anthropic Messages body to rawPredict with anthropicversion in the body. Vertex does not accept URL sources or strict tools. N..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexAnthropicProvider"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexAnthropicProvider : AnthropicProvider, IBatchProvider, IDecisionProvider, IEvaluationProvider
```


Anthropic on Vertex. Sends the Anthropic Messages body to `rawPredict` with `anthropic_version` in the body.
Vertex does not accept URL sources or strict tools. Native output is opt-in.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/), [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/), [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L73)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Google_GoogleVertexAnthropicOptions_"></a>

### GoogleVertexAnthropicProvider(HttpClient, GoogleVertexAnthropicOptions?)


```csharp
public GoogleVertexAnthropicProvider(HttpClient httpClient, GoogleVertexAnthropicOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [GoogleVertexAnthropicOptions](/api/Vercel.AI.Google/GoogleVertexAnthropicOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L86)

## Fields

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_ModelProviderName"></a>

### ModelProviderName


```csharp
public const string ModelProviderName = "googleVertex.anthropic.messages"
```


Provider id reported by language models.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L76)

## Properties

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


Publisher base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L118)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Location_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Location"></a>

### Location


```csharp
public string? Location { get; }
```


Location.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L115)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Project_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Project"></a>

### Project


```csharp
public string? Project { get; }
```


Project id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L112)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportedUrls_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportedUrls"></a>

### SupportedUrls


```csharp
protected override IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URL patterns that models of this provider accept for images and PDFs.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L136)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportsNativeStructuredOutput_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportsNativeStructuredOutput"></a>

### SupportsNativeStructuredOutput


```csharp
protected override bool SupportsNativeStructuredOutput { get; }
```


False when the host does not accept the native output format. Structured output then uses the JSON tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L121)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportsStrictTools_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportsStrictTools"></a>

### SupportsStrictTools


```csharp
protected override bool SupportsStrictTools { get; }
```


False when the host does not accept `strict` on tool definitions.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L133)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Tools_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Tools"></a>

### Tools


```csharp
public static IReadOnlyList<string> Tools { get; }
```


Provider tool ids that Vertex accepts. A subset of the Anthropic tools.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L96)

## Methods

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Create_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_Create_Vercel_AI_Google_GoogleVertexAnthropicOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(GoogleVertexAnthropicOptions?, HttpMessageHandler?)


```csharp
public static GoogleVertexAnthropicProvider Create(GoogleVertexAnthropicOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GoogleVertexAnthropicOptions](/api/Vercel.AI.Google/GoogleVertexAnthropicOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GoogleVertexAnthropicProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L139)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_CreateHeadersAsync_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_CreateHeadersAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_System_Threading_CancellationToken_"></a>

### `CreateHeadersAsync(IReadOnlyList<string>, IReadOnlyDictionary<string, string?>?, string?, CancellationToken)`


```csharp
public override Task<Dictionary<string, string?>> CreateHeadersAsync(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders, string? body, CancellationToken cancellationToken)
```


Generated bearer token, then configured headers, call headers, and beta flags.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `callHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L236)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public override IEmbeddingModel EmbeddingModel(string modelId)
```


Embedding models are not available on this provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L273)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_PredictUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_PredictUrl_System_String_System_Boolean_"></a>

### PredictUrl(string, bool)


```csharp
public string PredictUrl(string modelId, bool streaming)
```


`rawPredict` or `streamRawPredict` URL for a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `streaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L151)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_RequestUri_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_RequestUri_System_String_System_Boolean_"></a>

### RequestUri(string, bool)


```csharp
protected override Uri RequestUri(string modelId, bool streaming)
```


URL for one model call. Defaults to [MessagesUri()](/api/Vercel.AI.Anthropic/AnthropicProvider/#Vercel_AI_Anthropic_AnthropicProvider_MessagesUri).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `streaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L281)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportsNativeStructuredOutputFor_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_SupportsNativeStructuredOutputFor_System_String_"></a>

### SupportsNativeStructuredOutputFor(string)


```csharp
protected override bool SupportsNativeStructuredOutputFor(string modelId)
```


Per-model form of [SupportsNativeStructuredOutput](/api/Vercel.AI.Anthropic/AnthropicProvider/#Vercel_AI_Anthropic_AnthropicProvider_SupportsNativeStructuredOutput). Hosts that decide by model id override it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L126)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_TransformBody_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_TransformBody_System_Text_Json_Nodes_JsonObject_"></a>

### TransformBody(JsonObject)


```csharp
public static JsonObject TransformBody(JsonObject body)
```


Removes `model` and sets `anthropic_version`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L161)

<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_TransformHeaders_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicProvider_TransformHeaders_System_Collections_Generic_Dictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `TransformHeaders(Dictionary<string, string?>, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public static Dictionary<string, string?> TransformHeaders(Dictionary<string, string?> headers, IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Sends `providerOptions.vertex.labels` as the `x-vertex-ai-labels` header. An explicit header wins.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L170)
