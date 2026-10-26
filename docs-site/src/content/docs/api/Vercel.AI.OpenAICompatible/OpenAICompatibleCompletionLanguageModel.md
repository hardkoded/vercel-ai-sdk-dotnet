---
title: "OpenAICompatibleCompletionLanguageModel"
description: "Legacy Completions language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleCompletionLanguageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleCompletionLanguageModel : ILanguageModel
```


Legacy Completions language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L15)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_"></a>

### OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider, string)


```csharp
public OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider provider, string modelId)
```


Creates a completion model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L24)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_System_String_"></a>

### OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider, string, string?)


```csharp
public OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider provider, string modelId, string? providerId)
```


Creates a completion model with an explicit provider id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L30)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_System_String_System_String_"></a>

### OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider, string, string?, string?)


```csharp
public OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider provider, string modelId, string? providerId, string? requestModelId)
```


Creates a completion model whose reported [ModelId](#Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_ModelId) differs from the model sent to the API.
&lt;code class="paramref"&gt;requestModelId&lt;/code&gt;, when set, is used for the request `model` and the deployment route.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestModelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L39)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L57)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L51)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_ProviderOptionsName_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_ProviderOptionsName"></a>

### ProviderOptionsName


```csharp
public string ProviderOptionsName { get; }
```


Provider-options key. The segment before the first dot of [Provider](#Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_Provider).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L54)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L48)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L60)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleCompletionLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleCompletionLanguageModel.cs#L104)
