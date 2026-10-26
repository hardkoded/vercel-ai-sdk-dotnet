---
title: "AmazonBedrockLanguageModel"
description: "Bedrock Converse language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockLanguageModel"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockLanguageModel : ILanguageModel
```


Bedrock Converse language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L181)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel__ctor_Vercel_AI_AmazonBedrock_AmazonBedrockProvider_System_String_"></a>

### AmazonBedrockLanguageModel(AmazonBedrockProvider, string)


```csharp
public AmazonBedrockLanguageModel(AmazonBedrockProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L188)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_ModelId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L201)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_Provider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L198)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L195)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L208)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L225)
