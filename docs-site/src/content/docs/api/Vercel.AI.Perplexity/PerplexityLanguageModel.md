---
title: "PerplexityLanguageModel"
description: "Perplexity Agent API language model. Generate and stream requests post to /v1/agent. Sonar PDF input, video input, and image or video results have no Agent A..."
editUrl: false
prev: false
next: false
sidebar:
  label: "PerplexityLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel"></a>

Namespace: [Vercel.AI.Perplexity](/api/Vercel.AI.Perplexity/)


```csharp
public sealed class PerplexityLanguageModel : ILanguageModel
```


Perplexity Agent API language model. Generate and stream requests post to `/v1/agent`.
Sonar PDF input, video input, and image or video results have no Agent API equivalent and are not mapped.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L18)

## Constructors

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel__ctor_"></a>
<a id="Vercel_AI_Perplexity_PerplexityLanguageModel__ctor_Vercel_AI_Perplexity_PerplexityProvider_System_String_"></a>

### PerplexityLanguageModel(PerplexityProvider, string)


```csharp
public PerplexityLanguageModel(PerplexityProvider provider, string modelId)
```


Creates a language model for an Agent API preset or direct model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L23)

## Properties

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L36)

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_Provider_"></a>
<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L33)

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L30)

## Methods

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L39)

<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Perplexity_PerplexityLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityLanguageModel.cs#L97)
