---
title: "XaiResponsesLanguageModel"
description: "xAI Responses API language model. Generate and stream requests post to /responses. Pass xAI provider-defined tools with JsonElement) and the ids xai.websearc..."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiResponsesLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiResponsesLanguageModel : ILanguageModel
```


xAI Responses API language model. Generate and stream requests post to `/responses`.
Pass xAI provider-defined tools with `JsonElement)` and the ids `xai.web_search`,
`xai.x_search`, `xai.code_execution`, `xai.view_image`, `xai.view_x_video`, `xai.image_generation`,
`xai.file_search`, and `xai.mcp`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L31)

## Constructors

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiResponsesLanguageModel__ctor_Vercel_AI_Xai_XaiProvider_System_String_"></a>

### XaiResponsesLanguageModel(XaiProvider, string)


```csharp
public XaiResponsesLanguageModel(XaiProvider provider, string modelId)
```


Creates a Responses model for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L45)

## Properties

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L64)

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L61)

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L58)

## Methods

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L67)

<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Xai_XaiResponsesLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiResponsesLanguageModel.cs#L100)
