---
title: "AiClient"
description: "Default IAiClient. String model ids are resolved by the AI Gateway."
editUrl: false
prev: false
next: false
sidebar:
  label: "AiClient"
  hidden: true
---

<a id="Vercel_AI_AiClient"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class AiClient : IAiClient
```


Default [IAiClient](/api/Vercel.AI/IAiClient/). String model ids are resolved by the AI Gateway.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAiClient](/api/Vercel.AI/IAiClient/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L74)

## Constructors

<a id="Vercel_AI_AiClient__ctor_"></a>
<a id="Vercel_AI_AiClient__ctor_Vercel_AI_Gateway_GatewayProvider_Vercel_AI_IAiTelemetry_"></a>

### AiClient(GatewayProvider, IAiTelemetry?)


```csharp
public AiClient(GatewayProvider gateway, IAiTelemetry? telemetry = null)
```


Creates a client.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `gateway` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) |  |
| `telemetry` | [IAiTelemetry](/api/Vercel.AI/IAiTelemetry/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L80)

## Methods

<a id="Vercel_AI_AiClient_EmbedAsync_"></a>
<a id="Vercel_AI_AiClient_EmbedAsync_Vercel_AI_EmbedOptions_System_Threading_CancellationToken_"></a>

### EmbedAsync(EmbedOptions, CancellationToken)


```csharp
public Task<float[]> EmbedAsync(EmbedOptions options, CancellationToken cancellationToken = default)
```


Embeds one value. Maps to `embed`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [EmbedOptions](/api/Vercel.AI/EmbedOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L109)

<a id="Vercel_AI_AiClient_EmbedManyAsync_"></a>
<a id="Vercel_AI_AiClient_EmbedManyAsync_Vercel_AI_EmbedOptions_System_Threading_CancellationToken_"></a>

### EmbedManyAsync(EmbedOptions, CancellationToken)


```csharp
public Task<EmbeddingResult> EmbedManyAsync(EmbedOptions options, CancellationToken cancellationToken = default)
```


Embeds many values. Maps to `embedMany`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [EmbedOptions](/api/Vercel.AI/EmbedOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L121)

<a id="Vercel_AI_AiClient_EvaluateAsync_"></a>
<a id="Vercel_AI_AiClient_EvaluateAsync_Vercel_AI_EvaluateOptions_System_Threading_CancellationToken_"></a>

### EvaluateAsync(EvaluateOptions, CancellationToken)


```csharp
public Task<RubricResult> EvaluateAsync(EvaluateOptions options, CancellationToken cancellationToken = default)
```


Scores a candidate against a rubric. Uses [RubricModel](/api/Vercel.AI/EvaluateOptions/#Vercel_AI_EvaluateOptions_RubricModel) or a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [EvaluateOptions](/api/Vercel.AI/EvaluateOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RubricResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L225)

<a id="Vercel_AI_AiClient_GenerateImageAsync_"></a>
<a id="Vercel_AI_AiClient_GenerateImageAsync_Vercel_AI_GenerateImageOptions_System_Threading_CancellationToken_"></a>

### GenerateImageAsync(GenerateImageOptions, CancellationToken)


```csharp
public Task<ImageGenerationResult> GenerateImageAsync(GenerateImageOptions options, CancellationToken cancellationToken = default)
```


Generates images. Maps to `generateImage`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateImageOptions](/api/Vercel.AI/GenerateImageOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ImageGenerationResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L145)

<a id="Vercel_AI_AiClient_GenerateSpeechAsync_"></a>
<a id="Vercel_AI_AiClient_GenerateSpeechAsync_Vercel_AI_GenerateSpeechOptions_System_Threading_CancellationToken_"></a>

### GenerateSpeechAsync(GenerateSpeechOptions, CancellationToken)


```csharp
public Task<SpeechResult> GenerateSpeechAsync(GenerateSpeechOptions options, CancellationToken cancellationToken = default)
```


Generates speech. Maps to `generateSpeech`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateSpeechOptions](/api/Vercel.AI/GenerateSpeechOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L167)

<a id="Vercel_AI_AiClient_GenerateTextAsync_"></a>
<a id="Vercel_AI_AiClient_GenerateTextAsync_Vercel_AI_GenerateTextOptions_System_Threading_CancellationToken_"></a>

### GenerateTextAsync(GenerateTextOptions, CancellationToken)


```csharp
public Task<GenerateTextResult> GenerateTextAsync(GenerateTextOptions options, CancellationToken cancellationToken = default)
```


Generates text and runs tools. Maps to `generateText`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L87)

<a id="Vercel_AI_AiClient_GenerateVideoAsync_"></a>
<a id="Vercel_AI_AiClient_GenerateVideoAsync_Vercel_AI_GenerateVideoOptions_System_Threading_CancellationToken_"></a>

### GenerateVideoAsync(GenerateVideoOptions, CancellationToken)


```csharp
public Task<VideoResult> GenerateVideoAsync(GenerateVideoOptions options, CancellationToken cancellationToken = default)
```


Generates video. Maps to `generateVideo`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateVideoOptions](/api/Vercel.AI/GenerateVideoOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L214)

<a id="Vercel_AI_AiClient_RerankAsync_"></a>
<a id="Vercel_AI_AiClient_RerankAsync_Vercel_AI_RerankOptions_System_Threading_CancellationToken_"></a>

### RerankAsync(RerankOptions, CancellationToken)


```csharp
public Task<RerankResult> RerankAsync(RerankOptions options, CancellationToken cancellationToken = default)
```


Reranks documents. Maps to `rerank`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [RerankOptions](/api/Vercel.AI/RerankOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L134)

<a id="Vercel_AI_AiClient_StreamTextAsync_"></a>
<a id="Vercel_AI_AiClient_StreamTextAsync_Vercel_AI_StreamTextOptions_System_Threading_CancellationToken_"></a>

### StreamTextAsync(StreamTextOptions, CancellationToken)


```csharp
public StreamTextResult StreamTextAsync(StreamTextOptions options, CancellationToken cancellationToken = default)
```


Streams text and runs tools. Maps to `streamText`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L98)

<a id="Vercel_AI_AiClient_TranscribeAsync_"></a>
<a id="Vercel_AI_AiClient_TranscribeAsync_Vercel_AI_TranscribeOptions_System_Threading_CancellationToken_"></a>

### TranscribeAsync(TranscribeOptions, CancellationToken)


```csharp
public Task<TranscriptionResult> TranscribeAsync(TranscribeOptions options, CancellationToken cancellationToken = default)
```


Transcribes audio. Maps to `transcribe`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [TranscribeOptions](/api/Vercel.AI/TranscribeOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L185)

<a id="Vercel_AI_AiClient_TranslateAsync_"></a>
<a id="Vercel_AI_AiClient_TranslateAsync_Vercel_AI_TranslateOptions_System_Threading_CancellationToken_"></a>

### TranslateAsync(TranslateOptions, CancellationToken)


```csharp
public Task<TranscriptionResult> TranslateAsync(TranslateOptions options, CancellationToken cancellationToken = default)
```


Translates speech to text. Maps to `translate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [TranslateOptions](/api/Vercel.AI/TranslateOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L203)

<a id="Vercel_AI_AiClient_UploadFileAsync_"></a>
<a id="Vercel_AI_AiClient_UploadFileAsync_Vercel_AI_Provider_IFileStore_System_String_System_Byte___System_String_System_Threading_CancellationToken_"></a>

### UploadFileAsync(IFileStore, string, byte[], string, CancellationToken)


```csharp
public Task<UploadedFile> UploadFileAsync(IFileStore store, string fileName, byte[] data, string mediaType, CancellationToken cancellationToken = default)
```


Uploads a file through &lt;code class="paramref"&gt;store&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `store` | [IFileStore](/api/Vercel.AI.Provider/IFileStore/) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadedFile&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L261)

<a id="Vercel_AI_AiClient_UploadSkillAsync_"></a>
<a id="Vercel_AI_AiClient_UploadSkillAsync_Vercel_AI_Provider_ISkillStore_System_String_System_String_System_Threading_CancellationToken_"></a>

### UploadSkillAsync(ISkillStore, string, string, CancellationToken)


```csharp
public Task<UploadedSkill> UploadSkillAsync(ISkillStore store, string name, string instructions, CancellationToken cancellationToken = default)
```


Uploads a skill through &lt;code class="paramref"&gt;store&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `store` | [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadedSkill&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L272)
