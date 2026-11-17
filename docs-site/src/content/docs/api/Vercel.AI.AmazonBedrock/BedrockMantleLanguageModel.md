---
title: "BedrockMantleLanguageModel"
description: "Mantle language model. Delegates to the OpenAI Chat Completions or Responses model, with the request signed for bedrock-mantle."
editUrl: false
prev: false
next: false
sidebar:
  label: "BedrockMantleLanguageModel"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class BedrockMantleLanguageModel : ILanguageModel
```


Mantle language model. Delegates to the OpenAI Chat Completions or Responses model, with the request
signed for `bedrock-mantle`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L197)

## Constructors

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel__ctor_Vercel_AI_AmazonBedrock_BedrockMantleProvider_System_String_System_Boolean_"></a>

### BedrockMantleLanguageModel(BedrockMantleProvider, string, bool)


```csharp
public BedrockMantleLanguageModel(BedrockMantleProvider provider, string modelId, bool responses)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [BedrockMantleProvider](/api/Vercel.AI.AmazonBedrock/BedrockMantleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responses` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L203)

## Properties

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_ModelId_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L225)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_Provider_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L221)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L215)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URLs the model can read itself, keyed by media type pattern (such as `image/*`). Each value is a regular expression that a URL
must match. Prompt file URLs that match are passed through. All others are downloaded before the call. Maps to `supportedUrls`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L218)

## Methods

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L228)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L239)
