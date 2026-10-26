---
title: "OpenAIResponsesLanguageModel"
description: "OpenAI Responses API language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesLanguageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesLanguageModel : ILanguageModel
```


OpenAI Responses API language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L32)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel__ctor_Vercel_AI_Azure_AzureOpenAIProvider_System_String_"></a>

### OpenAIResponsesLanguageModel(AzureOpenAIProvider, string)


```csharp
public OpenAIResponsesLanguageModel(AzureOpenAIProvider provider, string modelId)
```


Creates an Azure OpenAI Responses model. Message and request options are read from the
`azure` provider options first and fall back to `openai` when `azure` is absent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L70)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel__ctor_Vercel_AI_Azure_AzureOpenAIProvider_System_String_Vercel_AI_Azure_AzureOpenAILanguageModelSettings_"></a>

### OpenAIResponsesLanguageModel(AzureOpenAIProvider, string, AzureOpenAILanguageModelSettings?)


```csharp
public OpenAIResponsesLanguageModel(AzureOpenAIProvider provider, string deploymentId, AzureOpenAILanguageModelSettings? settings)
```


Creates an Azure OpenAI Responses model for a deployment. [ModelId](#Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_ModelId) reports
&lt;code class="paramref"&gt;settings&lt;/code&gt;.[ModelId](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/#Vercel_AI_Azure_AzureOpenAILanguageModelSettings_ModelId) when set and
drives capability detection. Requests always use &lt;code class="paramref"&gt;deploymentId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/) |  |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L80)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIResponsesLanguageModel(OpenAIProvider, string)


```csharp
public OpenAIResponsesLanguageModel(OpenAIProvider provider, string modelId)
```


Creates a Responses model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L51)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L103)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L100)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L97)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_SupportsWebSearchSourcesInclude_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_SupportsWebSearchSourcesInclude"></a>

### SupportsWebSearchSourcesInclude


```csharp
public bool SupportsWebSearchSourcesInclude { get; init; }
```


Whether a request with a `web_search` or `web_search_preview` tool adds
`web_search_call.action.sources` to `include`. The default is true. The
`includeWebSearchSources` provider option set to false also turns it off.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L48)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L603)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L617)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_Prepare_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_"></a>

### Prepare(string, LanguageModelCallOptions, bool)


```csharp
public static OpenAIResponsesPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream)
```


Builds the Responses body for &lt;code class="paramref"&gt;modelId&lt;/code&gt;. Generate calls omit `stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `stream` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [OpenAIResponsesPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIResponsesPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L204)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_System_String_System_Boolean_"></a>

### Prepare(string, LanguageModelCallOptions, bool, string, bool)


```csharp
public static OpenAIResponsesPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream, string providerOptionsName, bool supportsWebSearchSourcesInclude = true)
```


Builds the Responses body. Options come from &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt; and fall back to
`openai` when that key is absent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `stream` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `supportsWebSearchSourcesInclude` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [OpenAIResponsesPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIResponsesPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L213)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_System_String_System_Boolean_System_String_"></a>

### Prepare(string, LanguageModelCallOptions, bool, string, bool, string?)


```csharp
public static OpenAIResponsesPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream, string providerOptionsName, bool supportsWebSearchSourcesInclude, string? requestModelId)
```


Builds the Responses body. &lt;code class="paramref"&gt;modelId&lt;/code&gt; drives capability detection and
&lt;code class="paramref"&gt;requestModelId&lt;/code&gt;, when set, is sent as the request `model`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `stream` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `supportsWebSearchSourcesInclude` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `requestModelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIResponsesPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIResponsesPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L222)

<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_PrepareResponsesTools_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesLanguageModel_PrepareResponsesTools_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_Vercel_AI_OpenAI_PrepareResponsesToolsOptions_"></a>

### `PrepareResponsesTools(IReadOnlyList<LanguageModelTool>?, ToolChoice?, PrepareResponsesToolsOptions?)`


```csharp
public static PreparedResponsesTools PrepareResponsesTools(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, PrepareResponsesToolsOptions? options = null)
```


Prepares Responses tools and the tool choice. Strict is false unless the tool sets [Strict](/api/Vercel.AI.Provider/LanguageModelTool/#Vercel_AI_Provider_LanguageModelTool_Strict).
Maps to `prepareResponsesTools`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `options` | [PrepareResponsesToolsOptions](/api/Vercel.AI.OpenAI/PrepareResponsesToolsOptions/) |  |

**Returns:** [PreparedResponsesTools](/api/Vercel.AI.OpenAI/PreparedResponsesTools/)

**Exceptions**

| Type | Condition |
| --- | --- |
| [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/) | The arguments of a provider-defined tool are invalid. |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | The tools or the tool choice cannot be expressed for OpenAI. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L111)
