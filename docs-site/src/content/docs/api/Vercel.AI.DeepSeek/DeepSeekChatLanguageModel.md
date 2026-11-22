---
title: "DeepSeekChatLanguageModel"
description: "DeepSeek Chat Completions language model. Maps to DeepSeekChatLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekChatLanguageModel"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekChatLanguageModel : ILanguageModel
```


DeepSeek Chat Completions language model. Maps to `DeepSeekChatLanguageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L64)

## Constructors

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel__ctor_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel__ctor_System_String_Vercel_AI_DeepSeek_DeepSeekChatConfig_"></a>

### DeepSeekChatLanguageModel(string, DeepSeekChatConfig)


```csharp
public DeepSeekChatLanguageModel(string modelId, DeepSeekChatConfig config)
```


Creates a chat model for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [DeepSeekChatConfig](/api/Vercel.AI.DeepSeek/DeepSeekChatConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L76)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_ModelId_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L89)

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_Provider_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L86)

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L83)

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URL patterns the model accepts without downloading: `image/*` over http or https. Maps to `supportedUrls`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L92)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L104)

<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L159)
