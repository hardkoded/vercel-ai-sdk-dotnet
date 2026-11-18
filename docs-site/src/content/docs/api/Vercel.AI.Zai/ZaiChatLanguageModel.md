---
title: "ZaiChatLanguageModel"
description: "Z.AI chat completions over the OpenAI-compatible protocol. Serializes its own config because the URL is rebuilt from the base URL when the model is restored."
editUrl: false
prev: false
next: false
sidebar:
  label: "ZaiChatLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Zai_ZaiChatLanguageModel"></a>

Namespace: [Vercel.AI.Zai](/api/Vercel.AI.Zai/)


```csharp
public sealed class ZaiChatLanguageModel : OpenAICompatibleLanguageModel, ILanguageModel
```


Z.AI chat completions over the OpenAI-compatible protocol.
Serializes its own config because the URL is rebuilt from the base URL when the model is restored.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiChatLanguageModel.cs#L15)

## Constructors

<a id="Vercel_AI_Zai_ZaiChatLanguageModel__ctor_"></a>
<a id="Vercel_AI_Zai_ZaiChatLanguageModel__ctor_Vercel_AI_Zai_ZaiProvider_System_String_"></a>

### ZaiChatLanguageModel(ZaiProvider, string)


```csharp
public ZaiChatLanguageModel(ZaiProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ZaiProvider](/api/Vercel.AI.Zai/ZaiProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiChatLanguageModel.cs#L20)

## Methods

<a id="Vercel_AI_Zai_ZaiChatLanguageModel_Deserialize_"></a>
<a id="Vercel_AI_Zai_ZaiChatLanguageModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static ZaiChatLanguageModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_Zai_ZaiChatLanguageModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `ZAI_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [ZaiChatLanguageModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiChatLanguageModel.cs#L54)

<a id="Vercel_AI_Zai_ZaiChatLanguageModel_Serialize_"></a>
<a id="Vercel_AI_Zai_ZaiChatLanguageModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and config for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.
The API key is not part of the serialized config. Pass it to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiChatLanguageModel.cs#L30)
