---
title: "OpenAICompletionLanguageModel"
description: "OpenAI legacy completions language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompletionLanguageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAICompletionLanguageModel : ILanguageModel
```


OpenAI legacy completions language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L31)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAICompletionLanguageModel(OpenAIProvider, string)


```csharp
public OpenAICompletionLanguageModel(OpenAIProvider provider, string modelId)
```


Creates a completion model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L36)

## Properties

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L49)

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L46)

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L43)

## Methods

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_ConvertPrompt_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_ConvertPrompt_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `ConvertPrompt(IReadOnlyList<ModelMessage>)`


```csharp
public static OpenAICompletionPrompt ConvertPrompt(IReadOnlyList<ModelMessage> prompt)
```


Converts messages into a completion prompt and the default stop sequence.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [OpenAICompletionPrompt](/api/Vercel.AI.OpenAI/OpenAICompletionPrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L175)

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L229)

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L264)

<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_Prepare_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionLanguageModel_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_"></a>

### Prepare(string, LanguageModelCallOptions, bool)


```csharp
public static OpenAICompletionPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream)
```


Builds the completions body. &lt;code class="paramref"&gt;stream&lt;/code&gt; adds streaming fields.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `stream` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [OpenAICompletionPreparedRequest](/api/Vercel.AI.OpenAI/OpenAICompletionPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L52)
