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
public sealed class OpenAICompatibleLanguageModel : ILanguageModel
```


Chat Completions language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L15)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L32)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L38)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Endpoint_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Endpoint"></a>

### Endpoint


```csharp
public Uri? Endpoint { get; set; }
```


When set, chat calls use this URL instead of the provider chat route.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L62)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L56)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L50)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ProviderOptionsName_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_ProviderOptionsName"></a>

### ProviderOptionsName


```csharp
public string ProviderOptionsName { get; }
```


Provider-options key. The segment before the first dot of [Provider](#Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_Provider).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L53)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L47)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SupportsStructuredOutputs_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_SupportsStructuredOutputs"></a>

### SupportsStructuredOutputs


```csharp
public bool SupportsStructuredOutputs { get; set; }
```


When true, JSON schema responses use `json_schema`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L59)

## Methods

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L65)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L177)
