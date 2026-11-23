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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1019)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1097)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1125)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1073)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1082)

<a id="Vercel_AI_Ai_GenerateTextAsync_"></a>
<a id="Vercel_AI_Ai_GenerateTextAsync_Vercel_AI_GenerateTextOptions_System_Threading_CancellationToken_"></a>

### GenerateTextAsync(GenerateTextOptions, CancellationToken)


```csharp
public static Task<GenerateTextResult<string>> GenerateTextAsync(GenerateTextOptions options, CancellationToken cancellationToken = default)
```


Generates text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateTextResult&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1026)

<a id="Vercel_AI_Ai_GenerateTextAsync__1_Vercel_AI_GenerateTextOptions_Vercel_AI_OutputSpec___0__System_Threading_CancellationToken_"></a>

### `GenerateTextAsync<TOutput>(GenerateTextOptions, OutputSpec<TOutput>, CancellationToken)`


```csharp
public static Task<GenerateTextResult<TOutput>> GenerateTextAsync<TOutput>(GenerateTextOptions options, OutputSpec<TOutput> output, CancellationToken cancellationToken = default)
```


Generates text whose output is typed by &lt;code class="paramref"&gt;output&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) |  |
| `output` | `OutputSpec&lt;TOutput&gt;` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateTextResult&lt;TOutput&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1035)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1091)

<a id="Vercel_AI_Ai_SafeValidateUIMessagesAsync_"></a>
<a id="Vercel_AI_Ai_SafeValidateUIMessagesAsync_Vercel_AI_Ui_ValidateUIMessagesOptions_"></a>

### SafeValidateUIMessagesAsync(ValidateUIMessagesOptions)


```csharp
public static Task<SafeValidateUIMessagesResult> SafeValidateUIMessagesAsync(ValidateUIMessagesOptions options)
```


Validates UI messages like [ValidateUIMessagesAsync(ValidateUIMessagesOptions)](#Vercel_AI_Ai_ValidateUIMessagesAsync_Vercel_AI_Ui_ValidateUIMessagesOptions_), but returns the error instead of throwing. Maps to `safeValidateUIMessages`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ValidateUIMessagesOptions](/api/Vercel.AI.Ui/ValidateUIMessagesOptions/) |  |

**Returns:** [Task&lt;SafeValidateUIMessagesResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1117)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1055)

<a id="Vercel_AI_Ai_StreamTextAsync__1_Vercel_AI_StreamTextOptions_Vercel_AI_OutputSpec___0__System_Threading_CancellationToken_"></a>

### `StreamTextAsync<TOutput>(StreamTextOptions, OutputSpec<TOutput>, CancellationToken)`


```csharp
public static StreamTextResult<TOutput> StreamTextAsync<TOutput>(StreamTextOptions options, OutputSpec<TOutput> output, CancellationToken cancellationToken = default)
```


Streams text whose output is typed by &lt;code class="paramref"&gt;output&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/) |  |
| `output` | `OutputSpec&lt;TOutput&gt;` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** `StreamTextResult&lt;TOutput&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1045)

<a id="Vercel_AI_Ai_StreamTextLowMemoryAsync_"></a>
<a id="Vercel_AI_Ai_StreamTextLowMemoryAsync_Vercel_AI_StreamTextOptions_System_Threading_CancellationToken_"></a>

### StreamTextLowMemoryAsync(StreamTextOptions, CancellationToken)


```csharp
public static StreamTextSingleConsumerResult StreamTextLowMemoryAsync(StreamTextOptions options, CancellationToken cancellationToken = default)
```


Streams text with a single consumer, without replay. Maps to `streamTextLowMemory`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [StreamTextSingleConsumerResult](/api/Vercel.AI/StreamTextSingleConsumerResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1064)

<a id="Vercel_AI_Ai_ValidateUIMessagesAsync_"></a>
<a id="Vercel_AI_Ai_ValidateUIMessagesAsync_Vercel_AI_Ui_ValidateUIMessagesOptions_"></a>

### ValidateUIMessagesAsync(ValidateUIMessagesOptions)


```csharp
public static Task<IReadOnlyList<UIMessage>> ValidateUIMessagesAsync(ValidateUIMessagesOptions options)
```


Validates a list of UI messages. Metadata, data parts, and tool parts are only validated if the corresponding schemas are
provided. Maps to `validateUIMessages`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ValidateUIMessagesOptions](/api/Vercel.AI.Ui/ValidateUIMessagesOptions/) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;UIMessage&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L1109)
