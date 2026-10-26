---
title: "HuggingFaceResponsesLanguageModel"
description: "Hugging Face Responses model. LanguageModel(string) stays on Chat Completions so existing chat clients keep working. Call ResponsesModel(string) for /responses."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceResponsesLanguageModel"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public sealed class HuggingFaceResponsesLanguageModel : ILanguageModel
```


Hugging Face Responses model. [LanguageModel(string)](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/#Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_LanguageModel_System_String_) stays on Chat Completions
so existing chat clients keep working. Call [ResponsesModel(string)](/api/Vercel.AI.HuggingFace/HuggingFaceProvider/#Vercel_AI_HuggingFace_HuggingFaceProvider_ResponsesModel_System_String_) for `/responses`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L18)

## Constructors

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel__ctor_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel__ctor_Vercel_AI_HuggingFace_HuggingFaceProvider_System_String_System_Func_System_String__"></a>

### `HuggingFaceResponsesLanguageModel(HuggingFaceProvider, string, Func<string>?)`


```csharp
public HuggingFaceResponsesLanguageModel(HuggingFaceProvider provider, string modelId, Func<string>? generateId = null)
```


Creates a responses model. &lt;code class="paramref"&gt;generateId&lt;/code&gt; names sources and defaults to random ids.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [HuggingFaceProvider](/api/Vercel.AI.HuggingFace/HuggingFaceProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `generateId` | [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L24)

## Properties

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_ModelId_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L38)

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_Provider_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L35)

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L32)

## Methods

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L41)

<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceResponsesLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceResponsesLanguageModel.cs#L64)
