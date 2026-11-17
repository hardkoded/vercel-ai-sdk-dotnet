---
title: "AzureSpeechModel"
description: "Azure speech model. Resolves the API for every call: providerOptions.azure.api wins, and otherwise MAI-Voice models use the Azure Speech API and every other..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureSpeechModel"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureSpeechModel : ISpeechModel
```


Azure speech model. Resolves the API for every call: `providerOptions.azure.api` wins, and otherwise
MAI-Voice models use the Azure Speech API and every other model uses the OpenAI speech API. Maps to
`AzureSpeechModel` in `azure-openai-provider.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L17)

## Properties

<a id="Vercel_AI_Azure_AzureSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L40)

<a id="Vercel_AI_Azure_AzureSpeechModel_Provider_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L37)

## Methods

<a id="Vercel_AI_Azure_AzureSpeechModel_Deserialize_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static AzureSpeechModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_Azure_AzureSpeechModel_Serialize), resolving the API for each call again. Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `AZURE_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [AzureSpeechModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L56)

<a id="Vercel_AI_Azure_AzureSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechCallOptions, CancellationToken)


```csharp
public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
```


Synthesizes speech.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SpeechCallOptions](/api/Vercel.AI.Provider/SpeechCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L68)

<a id="Vercel_AI_Azure_AzureSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_GenerateAsync_Vercel_AI_OpenAI_OpenAISpeechCall_System_Threading_CancellationToken_"></a>

### GenerateAsync(OpenAISpeechCall, CancellationToken)


```csharp
public Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, CancellationToken cancellationToken)
```


Synthesizes speech with the OpenAI or the Azure Speech API and returns the audio, warnings and response metadata.
Maps to `doGenerate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAISpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | An `azure` provider option is unknown or out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L91)

<a id="Vercel_AI_Azure_AzureSpeechModel_Serialize_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and the JSON-serializable provider settings for a workflow boundary. Maps to
`[WORKFLOW_SERIALIZE]`. The API key and the token provider are not part of the serialized config. Pass the key
to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModel.cs#L47)
