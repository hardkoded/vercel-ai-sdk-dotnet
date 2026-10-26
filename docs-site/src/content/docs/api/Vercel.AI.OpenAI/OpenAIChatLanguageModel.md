---
title: "OpenAIChatLanguageModel"
description: "OpenAI Chat Completions language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatLanguageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIChatLanguageModel : ILanguageModel
```


OpenAI Chat Completions language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L30)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel__ctor_Vercel_AI_Azure_AzureOpenAIProvider_System_String_Vercel_AI_Azure_AzureOpenAILanguageModelSettings_"></a>

### OpenAIChatLanguageModel(AzureOpenAIProvider, string, AzureOpenAILanguageModelSettings?)


```csharp
public OpenAIChatLanguageModel(AzureOpenAIProvider provider, string deploymentId, AzureOpenAILanguageModelSettings? settings)
```


Creates an Azure OpenAI Chat Completions model for a deployment. [ModelId](#Vercel_AI_OpenAI_OpenAIChatLanguageModel_ModelId) reports
&lt;code class="paramref"&gt;settings&lt;/code&gt;.[ModelId](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/#Vercel_AI_Azure_AzureOpenAILanguageModelSettings_ModelId) when set and
drives capability detection. Requests always use &lt;code class="paramref"&gt;deploymentId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/) |  |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L58)

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIChatLanguageModel(OpenAIProvider, string)


```csharp
public OpenAIChatLanguageModel(OpenAIProvider provider, string modelId)
```


Creates a Chat Completions model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L39)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L80)

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L77)

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L74)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L276)

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L290)

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_Prepare_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_"></a>

### Prepare(string, LanguageModelCallOptions)


```csharp
public static OpenAIChatPreparedRequest Prepare(string modelId, LanguageModelCallOptions options)
```


Builds the Chat Completions body for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |

**Returns:** [OpenAIChatPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIChatPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L83)

<a id="Vercel_AI_OpenAI_OpenAIChatLanguageModel_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_String_"></a>

### Prepare(string, LanguageModelCallOptions, string?)


```csharp
public static OpenAIChatPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, string? requestModelId)
```


Builds the Chat Completions body. &lt;code class="paramref"&gt;modelId&lt;/code&gt; drives capability detection and
&lt;code class="paramref"&gt;requestModelId&lt;/code&gt;, when set, is sent as the request `model`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `requestModelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIChatPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIChatPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L92)
