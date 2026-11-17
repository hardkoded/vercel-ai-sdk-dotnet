---
title: "OpenAICompatibleLanguageModel"
description: "Chat Completions language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleLanguageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public class OpenAICompatibleLanguageModel : ILanguageModel
```


Chat Completions language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

**Derived:** [TencentChatLanguageModel](/api/Vercel.AI.Tencent/TencentChatLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L16)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_"></a>

### OpenAICompatibleLanguageModel(OpenAICompatibleProvider, string)


```csharp
public OpenAICompatibleLanguageModel(OpenAICompatibleProvider provider, string modelId)
```


Creates a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L33)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_System_String_"></a>

### OpenAICompatibleLanguageModel(OpenAICompatibleProvider, string, string?)


```csharp
public OpenAICompatibleLanguageModel(OpenAICompatibleProvider provider, string modelId, string? providerId)
```


Creates a language model with an explicit provider id such as `anthropic.beta`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L37)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Endpoint_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Endpoint"></a>

### Endpoint


```csharp
public Uri? Endpoint { get; set; }
```


When set, chat calls use this URL instead of the provider chat route.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L70)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L64)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L58)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ProviderOptionsName_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ProviderOptionsName"></a>

### ProviderOptionsName


```csharp
public string ProviderOptionsName { get; }
```


Provider-options key. The segment before the first dot of [Provider](#Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Provider).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L61)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L51)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URLs the model can read itself, keyed by media type pattern (such as `image/*`). Each value is a regular expression that a URL
must match. Prompt file URLs that match are passed through. All others are downloaded before the call. Maps to `supportedUrls`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L54)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SupportsStructuredOutputs_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SupportsStructuredOutputs"></a>

### SupportsStructuredOutputs


```csharp
public bool SupportsStructuredOutputs { get; set; }
```


When true, JSON schema responses use `json_schema`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L67)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ConvertResponse_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ConvertResponse_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `ConvertResponse(JsonElement, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public LanguageModelGenerateResult ConvertResponse(JsonElement body, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Converts a Chat Completions response body into a generate result, without a network call. The result has no
warnings, raw response or response headers. Throws [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/) when the body is not a
response object and [InvalidResponseDataException](/api/Vercel.AI.Operations/InvalidResponseDataException/) when it has no choices.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) | The unvalidated response body. |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | The request's provider options, so the provider metadata key matches `CancellationToken)`. Null uses the provider name. |

**Returns:** [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/)

Maps to `convertResponse`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L91)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Generates a complete result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L120)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoStreamAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Streams partial results. The result carries the stream with the request and response info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L303)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_PrepareRequest_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_PrepareRequest_Vercel_AI_Provider_LanguageModelCallOptions_"></a>

### PrepareRequest(LanguageModelCallOptions)


```csharp
public OpenAICompatiblePreparedRequest PrepareRequest(LanguageModelCallOptions options)
```


Builds the body and warnings `CancellationToken)` would send, without a network call or `stream`
fields. Call headers and the cancellation token are not applied.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |

**Returns:** [OpenAICompatiblePreparedRequest](/api/Vercel.AI.OpenAICompatible/OpenAICompatiblePreparedRequest/)

Maps to `prepareRequest`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L77)
