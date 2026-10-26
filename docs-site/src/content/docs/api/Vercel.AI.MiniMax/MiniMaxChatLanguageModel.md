---
title: "MiniMaxChatLanguageModel"
description: "MiniMax chat language model. Reuses the Anthropic Messages adapter and adds the M3.1 and later capabilities."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxChatLanguageModel"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxChatLanguageModel : AnthropicLanguageModel, ILanguageModel
```


MiniMax chat language model. Reuses the Anthropic Messages adapter and adds the M3.1 and later capabilities.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AnthropicLanguageModel](/api/Vercel.AI.Anthropic/AnthropicLanguageModel/)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxChatLanguageModel.cs#L16)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel__ctor_Vercel_AI_Anthropic_AnthropicProvider_System_String_"></a>

### MiniMaxChatLanguageModel(AnthropicProvider, string)


```csharp
public MiniMaxChatLanguageModel(AnthropicProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxChatLanguageModel.cs#L35)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_SupportedUrls_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_SupportedUrls"></a>

### SupportedUrls


```csharp
public override IReadOnlyDictionary<string, string> SupportedUrls { get; }
```


URL patterns the model accepts for images and PDFs.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxChatLanguageModel.cs#L109)

## Methods

<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_Deserialize_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static MiniMaxChatLanguageModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_MiniMax_MiniMaxChatLanguageModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `MINIMAX_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [MiniMaxChatLanguageModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxChatLanguageModel.cs#L84)

<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_PrepareCallOptions_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_PrepareCallOptions_Vercel_AI_Provider_LanguageModelCallOptions_System_Collections_Generic_IList_Vercel_AI_Anthropic_AnthropicWarning__"></a>

### `PrepareCallOptions(LanguageModelCallOptions, IList<AnthropicWarning>)`


```csharp
protected override LanguageModelCallOptions PrepareCallOptions(LanguageModelCallOptions options, IList<AnthropicWarning> warnings)
```


Lets a compatible provider rewrite the call options before the request is built.
Warnings added to &lt;code class="paramref"&gt;warnings&lt;/code&gt; are reported after the request's own warnings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `warnings` | [IList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxChatLanguageModel.cs#L112)

<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_Serialize_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxChatLanguageModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and config for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.
The API key is not part of the serialized config. Pass it to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxChatLanguageModel.cs#L59)
