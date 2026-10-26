---
title: "AmazonBedrockAnthropicLanguageModel"
description: "Anthropic language model invoked through Bedrock. Delegates to AnthropicLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockAnthropicLanguageModel"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockAnthropicLanguageModel : ILanguageModel
```


Anthropic language model invoked through Bedrock. Delegates to [AnthropicLanguageModel](/api/Vercel.AI.Anthropic/AnthropicLanguageModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L321)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel__ctor_Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_System_String_"></a>

### AmazonBedrockAnthropicLanguageModel(AmazonBedrockAnthropicProvider, string)


```csharp
public AmazonBedrockAnthropicLanguageModel(AmazonBedrockAnthropicProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AmazonBedrockAnthropicProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L326)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_ModelId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L339)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_Provider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L336)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L333)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L342)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L348)
