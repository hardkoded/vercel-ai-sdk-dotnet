---
title: "MistralConversationLanguageModel"
description: "Mistral Conversations API language model, with support for web search. Maps to MistralConversationLanguageModel in mistral-conversation-language-model.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralConversationLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralConversationLanguageModel : ILanguageModel
```


Mistral Conversations API language model, with support for web search. Maps to `MistralConversationLanguageModel`
in `mistral-conversation-language-model.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L21)

## Constructors

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_"></a>

### MistralConversationLanguageModel(OpenAICompatibleProvider, string)


```csharp
public MistralConversationLanguageModel(OpenAICompatibleProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L40)

## Properties

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L54)

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_Provider_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L51)

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L48)

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URLs the model can read itself, keyed by media type pattern (such as `image/*`). Each value is a regular expression that a URL
must match. Prompt file URLs that match are passed through. All others are downloaded before the call. Maps to `supportedUrls`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L57)

## Methods

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L62)

<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Mistral_MistralConversationLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralConversationLanguageModel.cs#L163)
