---
title: "AnthropicLanguageModel"
description: "Anthropic Messages language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public class AnthropicLanguageModel : ILanguageModel
```


Anthropic Messages language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

**Derived:** [MiniMaxChatLanguageModel](/api/Vercel.AI.MiniMax/MiniMaxChatLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L474)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel__ctor_Vercel_AI_Anthropic_AnthropicProvider_System_String_"></a>

### AnthropicLanguageModel(AnthropicProvider, string)


```csharp
public AnthropicLanguageModel(AnthropicProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L480)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel__ctor_Vercel_AI_Anthropic_AnthropicProvider_System_String_Vercel_AI_Anthropic_AnthropicModelSettings_"></a>

### AnthropicLanguageModel(AnthropicProvider, string, AnthropicModelSettings?)


```csharp
public AnthropicLanguageModel(AnthropicProvider provider, string modelId, AnthropicModelSettings? settings)
```


Creates a model for a Messages-compatible provider that adjusts the adapter with &lt;code class="paramref"&gt;settings&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AnthropicModelSettings](/api/Vercel.AI.Anthropic/AnthropicModelSettings/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L486)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L500)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L497)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L494)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public virtual IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URL patterns the model accepts for images and PDFs.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L503)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public virtual Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Generates a complete result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L513)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoStreamAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public virtual Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Streams partial results. The result carries the stream with the request and response info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L524)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_PrepareCallOptions_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_PrepareCallOptions_Vercel_AI_Provider_LanguageModelCallOptions_System_Collections_Generic_IList_Vercel_AI_Anthropic_AnthropicWarning__"></a>

### `PrepareCallOptions(LanguageModelCallOptions, IList<AnthropicWarning>)`


```csharp
protected virtual LanguageModelCallOptions PrepareCallOptions(LanguageModelCallOptions options, IList<AnthropicWarning> warnings)
```


Lets a compatible provider rewrite the call options before the request is built.
Warnings added to &lt;code class="paramref"&gt;warnings&lt;/code&gt; are reported after the request's own warnings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `warnings` | [IList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L566)

<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_SupportsUrl_"></a>
<a id="Vercel_AI_Anthropic_AnthropicLanguageModel_SupportsUrl_System_String_System_String_"></a>

### SupportsUrl(string, string)


```csharp
public bool SupportsUrl(string mediaType, string url)
```


True when &lt;code class="paramref"&gt;url&lt;/code&gt; is an http or https URL for a supported media type.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L506)
