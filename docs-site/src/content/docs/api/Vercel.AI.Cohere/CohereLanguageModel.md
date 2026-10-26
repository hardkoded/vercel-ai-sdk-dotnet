---
title: "CohereLanguageModel"
description: "Cohere v2 chat model."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereLanguageModel"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereLanguageModel : ILanguageModel
```


Cohere v2 chat model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L17)

## Constructors

<a id="Vercel_AI_Cohere_CohereLanguageModel__ctor_"></a>
<a id="Vercel_AI_Cohere_CohereLanguageModel__ctor_Vercel_AI_Cohere_CohereProvider_System_String_"></a>

### CohereLanguageModel(CohereProvider, string)


```csharp
public CohereLanguageModel(CohereProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [CohereProvider](/api/Vercel.AI.Cohere/CohereProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L24)

## Properties

<a id="Vercel_AI_Cohere_CohereLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Cohere_CohereLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L37)

<a id="Vercel_AI_Cohere_CohereLanguageModel_Provider_"></a>
<a id="Vercel_AI_Cohere_CohereLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L34)

<a id="Vercel_AI_Cohere_CohereLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Cohere_CohereLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L31)

## Methods

<a id="Vercel_AI_Cohere_CohereLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Cohere_CohereLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L42)

<a id="Vercel_AI_Cohere_CohereLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Cohere_CohereLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereLanguageModel.cs#L95)
