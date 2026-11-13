---
title: "DeepSeekResponsesLanguageModel"
description: "DeepSeek Responses API language model. Generate and stream requests post to /responses. It supports the server-side WebSearch(string) tool. Maps to DeepSeekR..."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekResponsesLanguageModel"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekResponsesLanguageModel : ILanguageModel
```


DeepSeek Responses API language model. Generate and stream requests post to `/responses`.
It supports the server-side [WebSearch(string)](/api/Vercel.AI.DeepSeek/DeepSeekTools/#Vercel_AI_DeepSeek_DeepSeekTools_WebSearch_System_String_) tool. Maps to `DeepSeekResponsesLanguageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L19)

## Constructors

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel__ctor_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel__ctor_Vercel_AI_DeepSeek_DeepSeekProvider_System_String_"></a>

### DeepSeekResponsesLanguageModel(DeepSeekProvider, string)


```csharp
public DeepSeekResponsesLanguageModel(DeepSeekProvider provider, string modelId)
```


Creates a Responses model for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [DeepSeekProvider](/api/Vercel.AI.DeepSeek/DeepSeekProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L34)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_ModelId_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L47)

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_Provider_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L44)

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L41)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L52)

<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekResponsesLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekResponsesLanguageModel.cs#L86)
