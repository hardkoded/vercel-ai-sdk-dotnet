---
title: "OpenResponsesLanguageModel"
description: "Posts to /responses."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesLanguageModel"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesLanguageModel : ILanguageModel
```


Posts to `/responses`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L112)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel__ctor_Vercel_AI_OpenResponses_OpenResponsesProvider_System_String_"></a>

### OpenResponsesLanguageModel(OpenResponsesProvider, string)


```csharp
public OpenResponsesLanguageModel(OpenResponsesProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenResponsesProvider](/api/Vercel.AI.OpenResponses/OpenResponsesProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L130)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L147)

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L144)

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L137)

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URLs the model can read itself, keyed by media type pattern (such as `image/*`). Each value is a regular expression that a URL
must match. Prompt file URLs that match are passed through. All others are downloaded before the call. Maps to `supportedUrls`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L140)

## Methods

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L174)

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L209)

<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_Serialize_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesLanguageModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and config for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.
Throws a [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/) when extensions are registered, because their codecs cannot cross the boundary.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L155)
