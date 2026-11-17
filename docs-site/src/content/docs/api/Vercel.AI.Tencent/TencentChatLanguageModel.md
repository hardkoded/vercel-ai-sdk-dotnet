---
title: "TencentChatLanguageModel"
description: "TokenHub chat completions over the OpenAI-compatible protocol. Serializes its own config because the URL is rebuilt from the base URL when the model is resto..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentChatLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentChatLanguageModel"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentChatLanguageModel : OpenAICompatibleLanguageModel, ILanguageModel
```


TokenHub chat completions over the OpenAI-compatible protocol.
Serializes its own config because the URL is rebuilt from the base URL when the model is restored.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentChatLanguageModel.cs#L15)

## Constructors

<a id="Vercel_AI_Tencent_TencentChatLanguageModel__ctor_"></a>
<a id="Vercel_AI_Tencent_TencentChatLanguageModel__ctor_Vercel_AI_Tencent_TencentProvider_System_String_"></a>

### TencentChatLanguageModel(TencentProvider, string)


```csharp
public TencentChatLanguageModel(TencentProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [TencentProvider](/api/Vercel.AI.Tencent/TencentProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentChatLanguageModel.cs#L20)

## Methods

<a id="Vercel_AI_Tencent_TencentChatLanguageModel_Deserialize_"></a>
<a id="Vercel_AI_Tencent_TencentChatLanguageModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static TencentChatLanguageModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_Tencent_TencentChatLanguageModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `TENCENT_TOKENHUB_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [TencentChatLanguageModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentChatLanguageModel.cs#L54)

<a id="Vercel_AI_Tencent_TencentChatLanguageModel_Serialize_"></a>
<a id="Vercel_AI_Tencent_TencentChatLanguageModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and config for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.
The API key is not part of the serialized config. Pass it to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentChatLanguageModel.cs#L30)
