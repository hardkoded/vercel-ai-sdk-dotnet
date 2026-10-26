---
title: "ProdiaLanguageModel"
description: "Prodia image-to-image job exposed as a language model. The last user message supplies the text and the optional input image. The result holds the returned te..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProdiaLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Prodia_ProdiaLanguageModel"></a>

Namespace: [Vercel.AI.Prodia](/api/Vercel.AI.Prodia/)


```csharp
public sealed class ProdiaLanguageModel : ILanguageModel
```


Prodia image-to-image job exposed as a language model. The last user message supplies the text and the
optional input image. The result holds the returned text and images.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L20)

## Constructors

<a id="Vercel_AI_Prodia_ProdiaLanguageModel__ctor_"></a>
<a id="Vercel_AI_Prodia_ProdiaLanguageModel__ctor_Vercel_AI_Prodia_ProdiaProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `ProdiaLanguageModel(ProdiaProvider, string, Func<DateTimeOffset>?)`


```csharp
public ProdiaLanguageModel(ProdiaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ProdiaProvider](/api/Vercel.AI.Prodia/ProdiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L28)

## Properties

<a id="Vercel_AI_Prodia_ProdiaLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Prodia_ProdiaLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L42)

<a id="Vercel_AI_Prodia_ProdiaLanguageModel_Provider_"></a>
<a id="Vercel_AI_Prodia_ProdiaLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L39)

<a id="Vercel_AI_Prodia_ProdiaLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Prodia_ProdiaLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L36)

## Methods

<a id="Vercel_AI_Prodia_ProdiaLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L45)

<a id="Vercel_AI_Prodia_ProdiaLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoStreamAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Runs `CancellationToken)` and replays the result as stream parts. Generated images are not streamed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaLanguageModel.cs#L117)
