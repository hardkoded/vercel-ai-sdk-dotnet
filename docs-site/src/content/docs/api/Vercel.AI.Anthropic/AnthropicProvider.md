---
title: "AnthropicProvider"
description: "Anthropic Messages provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicProvider"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicProvider"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public class AnthropicProvider : ProviderBase, IBatchProvider, IDecisionProvider, IEvaluationProvider
```


Anthropic Messages provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/), [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/), [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/)

**Derived:** [AnthropicAwsProvider](/api/Vercel.AI.Anthropic/AnthropicAwsProvider/), [GoogleVertexAnthropicProvider](/api/Vercel.AI.Google/GoogleVertexAnthropicProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L79)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicProvider__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Anthropic_AnthropicOptions_"></a>

### AnthropicProvider(HttpClient, AnthropicOptions?)


```csharp
public AnthropicProvider(HttpClient httpClient, AnthropicOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [AnthropicOptions](/api/Vercel.AI.Anthropic/AnthropicOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L116)

## Fields

<a id="Vercel_AI_Anthropic_AnthropicProvider_ProviderName"></a>

### ProviderName


```csharp
public const string ProviderName = "anthropic"
```


Provider id used for dependency injection.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L82)

<a id="Vercel_AI_Anthropic_AnthropicProvider_UserAgent"></a>

### UserAgent


```csharp
public static readonly string UserAgent
```


User agent for the Messages API.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L113)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicProvider_Aws_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Aws"></a>

### Aws


```csharp
protected bool Aws { get; set; }
```


True for Claude Platform on AWS.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L140)

<a id="Vercel_AI_Anthropic_AnthropicProvider_ExplicitBaseUrl_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_ExplicitBaseUrl"></a>

### ExplicitBaseUrl


```csharp
protected bool ExplicitBaseUrl { get; set; }
```


True when the caller supplied a base URL other than the public Anthropic host.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L143)

<a id="Vercel_AI_Anthropic_AnthropicProvider_Http_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L137)

<a id="Vercel_AI_Anthropic_AnthropicProvider_ModelProvider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_ModelProvider"></a>

### ModelProvider


```csharp
public string ModelProvider { get; }
```


Provider id reported by language models.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L146)

<a id="Vercel_AI_Anthropic_AnthropicProvider_Options_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Options"></a>

### Options


```csharp
public AnthropicOptions Options { get; }
```


Options.

**Value:** [AnthropicOptions](/api/Vercel.AI.Anthropic/AnthropicOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L134)

<a id="Vercel_AI_Anthropic_AnthropicProvider_SupportedUrls_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_SupportedUrls"></a>

### SupportedUrls


```csharp
protected virtual IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URL patterns that models of this provider accept for images and PDFs.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L275)

<a id="Vercel_AI_Anthropic_AnthropicProvider_SupportsNativeStructuredOutput_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_SupportsNativeStructuredOutput"></a>

### SupportsNativeStructuredOutput


```csharp
protected virtual bool SupportsNativeStructuredOutput { get; }
```


False when the host does not accept the native output format. Structured output then uses the JSON tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L269)

<a id="Vercel_AI_Anthropic_AnthropicProvider_SupportsStrictTools_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_SupportsStrictTools"></a>

### SupportsStrictTools


```csharp
protected virtual bool SupportsStrictTools { get; }
```


False when the host does not accept `strict` on tool definitions.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L272)

<a id="Vercel_AI_Anthropic_AnthropicProvider_Tools_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Tools"></a>

### Tools


```csharp
public static IReadOnlyList<string> Tools { get; }
```


Provider tool ids, such as `anthropic.web_search_20250305`. Pass one as a provider tool id.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L85)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicProvider_Create_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Create_Vercel_AI_Anthropic_AnthropicOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(AnthropicOptions?, HttpMessageHandler?)


```csharp
public static AnthropicProvider Create(AnthropicOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [AnthropicOptions](/api/Vercel.AI.Anthropic/AnthropicOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [AnthropicProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L160)

<a id="Vercel_AI_Anthropic_AnthropicProvider_CreateHeaders_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_CreateHeaders_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `CreateHeaders(IReadOnlyList<string>, IReadOnlyDictionary<string, string?>?)`


```csharp
public Dictionary<string, string?> CreateHeaders(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders)
```


Headers for a Messages call that already has an API key or auth token.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `callHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L305)

<a id="Vercel_AI_Anthropic_AnthropicProvider_CreateHeadersAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_CreateHeadersAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_System_Threading_CancellationToken_"></a>

### `CreateHeadersAsync(IReadOnlyList<string>, IReadOnlyDictionary<string, string?>?, string?, CancellationToken)`


```csharp
public virtual Task<Dictionary<string, string?>> CreateHeadersAsync(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders, string? body, CancellationToken cancellationToken)
```


Headers for a Messages call. Signs the body when AWS credentials are used.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `callHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L311)

<a id="Vercel_AI_Anthropic_AnthropicProvider_DecisionModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public DecisionLanguageModel DecisionModel(string modelId)
```


Decision model. Wraps the Messages language model with structured output. The provider id is `anthropic.decision`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionLanguageModel](/api/Vercel.AI.Operations/DecisionLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L202)

<a id="Vercel_AI_Anthropic_AnthropicProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L184)

<a id="Vercel_AI_Anthropic_AnthropicProvider_EvaluationModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_EvaluationModel_System_String_"></a>

### EvaluationModel(string)


```csharp
[Obsolete("Use DecisionModel instead.")]
public DecisionLanguageModel EvaluationModel(string modelId)
```


Deprecated alias of [DecisionModel(string)](#Vercel_AI_Anthropic_AnthropicProvider_DecisionModel_System_String_). Maps to `evaluationModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionLanguageModel](/api/Vercel.AI.Operations/DecisionLanguageModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L213)

<a id="Vercel_AI_Anthropic_AnthropicProvider_ExperimentalBatch_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_ExperimentalBatch"></a>

### ExperimentalBatch()


```csharp
public IBatchApi? ExperimentalBatch()
```


Message Batches API. Downloads batch results.

**Returns:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L227)

<a id="Vercel_AI_Anthropic_AnthropicProvider_Files_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Files"></a>

### Files()


```csharp
public AnthropicFiles Files()
```


Files API client.

**Returns:** [AnthropicFiles](/api/Vercel.AI.Anthropic/AnthropicFiles/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L233)

<a id="Vercel_AI_Anthropic_AnthropicProvider_ImageModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L190)

<a id="Vercel_AI_Anthropic_AnthropicProvider_LanguageModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L167)

<a id="Vercel_AI_Anthropic_AnthropicProvider_MessagesUri_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_MessagesUri"></a>

### MessagesUri()


```csharp
public Uri MessagesUri()
```


Messages URL for the resolved base.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L245)

<a id="Vercel_AI_Anthropic_AnthropicProvider_New_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_New_System_String_"></a>

### New(string)


```csharp
public ILanguageModel New(string modelId)
```


Throws because the JavaScript provider function rejects `new`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L178)

<a id="Vercel_AI_Anthropic_AnthropicProvider_NormalizedBase_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_NormalizedBase"></a>

### NormalizedBase()


```csharp
public string NormalizedBase()
```


Resolved API origin, including `/v1` when the host is the public Anthropic API or a regional AWS host.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L282)

<a id="Vercel_AI_Anthropic_AnthropicProvider_RequestUri_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_RequestUri_System_String_System_Boolean_"></a>

### RequestUri(string, bool)


```csharp
protected virtual Uri RequestUri(string modelId, bool streaming)
```


URL for one model call. Defaults to [MessagesUri()](#Vercel_AI_Anthropic_AnthropicProvider_MessagesUri).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `streaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L257)

<a id="Vercel_AI_Anthropic_AnthropicProvider_RubricModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_RubricModel_System_String_"></a>

### RubricModel(string)


```csharp
public override IRubricModel RubricModel(string modelId)
```


Returns a model that scores a candidate against a rubric.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L196)

<a id="Vercel_AI_Anthropic_AnthropicProvider_Skills_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_Skills"></a>

### Skills()


```csharp
public AnthropicSkills Skills()
```


Skills API client.

**Returns:** [AnthropicSkills](/api/Vercel.AI.Anthropic/AnthropicSkills/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L239)

<a id="Vercel_AI_Anthropic_AnthropicProvider_TransformRequestBody_"></a>
<a id="Vercel_AI_Anthropic_AnthropicProvider_TransformRequestBody_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `TransformRequestBody(JsonObject, IReadOnlyList<string>)`


```csharp
protected virtual JsonObject TransformRequestBody(JsonObject body, IReadOnlyList<string> betas)
```


Rewrites the Messages body just before it is sent. Defaults to [TransformRequestBody](/api/Vercel.AI.Anthropic/AnthropicOptions/#Vercel_AI_Anthropic_AnthropicOptions_TransformRequestBody).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L263)
