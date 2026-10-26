---
title: "Ai"
description: "Static entry points. String model ids use the AI Gateway and AIGATEWAYAPIKEY."
editUrl: false
prev: false
next: false
sidebar:
  label: "Ai"
  hidden: true
---

<a id="Vercel_AI_Ai"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class Ai
```


Static entry points. String model ids use the AI Gateway and `AI_GATEWAY_API_KEY`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L460)

## Methods

<a id="Vercel_AI_Ai_ConvertToModelMessages_"></a>
<a id="Vercel_AI_Ai_ConvertToModelMessages_System_Collections_Generic_IReadOnlyList_Vercel_AI_Ui_UIMessage__Vercel_AI_Ui_ConvertToModelMessagesOptions_"></a>

### `ConvertToModelMessages(IReadOnlyList<UIMessage>, ConvertToModelMessagesOptions?)`


```csharp
public static IReadOnlyList<ModelMessage> ConvertToModelMessages(IReadOnlyList<UIMessage> messages, ConvertToModelMessagesOptions? options = null)
```


Converts UI messages from a chat UI into model messages. Maps to `convertToModelMessages`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `options` | [ConvertToModelMessagesOptions](/api/Vercel.AI.Ui/ConvertToModelMessagesOptions/) |  |

**Returns:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L495)

<a id="Vercel_AI_Ai_CosineSimilarity_"></a>
<a id="Vercel_AI_Ai_CosineSimilarity_System_Collections_Generic_IReadOnlyList_System_Single__System_Collections_Generic_IReadOnlyList_System_Single__"></a>

### `CosineSimilarity(IReadOnlyList<float>, IReadOnlyList<float>)`


```csharp
public static double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
```


Cosine similarity of two vectors.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `left` | [IReadOnlyList&lt;float&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `right` | [IReadOnlyList&lt;float&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L501)

<a id="Vercel_AI_Ai_EmbedAsync_"></a>
<a id="Vercel_AI_Ai_EmbedAsync_Vercel_AI_EmbedOptions_System_Threading_CancellationToken_"></a>

### EmbedAsync(EmbedOptions, CancellationToken)


```csharp
public static Task<float[]> EmbedAsync(EmbedOptions options, CancellationToken cancellationToken = default)
```


Embeds one value.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [EmbedOptions](/api/Vercel.AI/EmbedOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L477)

<a id="Vercel_AI_Ai_EmbedManyAsync_"></a>
<a id="Vercel_AI_Ai_EmbedManyAsync_Vercel_AI_EmbedOptions_System_Threading_CancellationToken_"></a>

### EmbedManyAsync(EmbedOptions, CancellationToken)


```csharp
public static Task<EmbeddingResult> EmbedManyAsync(EmbedOptions options, CancellationToken cancellationToken = default)
```


Embeds many values.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [EmbedOptions](/api/Vercel.AI/EmbedOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L483)

<a id="Vercel_AI_Ai_GenerateTextAsync_"></a>
<a id="Vercel_AI_Ai_GenerateTextAsync_Vercel_AI_GenerateTextOptions_System_Threading_CancellationToken_"></a>

### GenerateTextAsync(GenerateTextOptions, CancellationToken)


```csharp
public static Task<GenerateTextResult> GenerateTextAsync(GenerateTextOptions options, CancellationToken cancellationToken = default)
```


Generates text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L465)

<a id="Vercel_AI_Ai_PruneMessages_"></a>
<a id="Vercel_AI_Ai_PruneMessages_Vercel_AI_GenerateText_PruneMessagesOptions_"></a>

### PruneMessages(PruneMessagesOptions)


```csharp
public static IReadOnlyList<ModelMessage> PruneMessages(PruneMessagesOptions options)
```


Prunes reasoning, tool calls, results, errors, and approvals from messages. Maps to `pruneMessages`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [PruneMessagesOptions](/api/Vercel.AI.GenerateText/PruneMessagesOptions/) |  |

**Returns:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L489)

<a id="Vercel_AI_Ai_StreamTextAsync_"></a>
<a id="Vercel_AI_Ai_StreamTextAsync_Vercel_AI_StreamTextOptions_System_Threading_CancellationToken_"></a>

### StreamTextAsync(StreamTextOptions, CancellationToken)


```csharp
public static StreamTextResult StreamTextAsync(StreamTextOptions options, CancellationToken cancellationToken = default)
```


Streams text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L471)
