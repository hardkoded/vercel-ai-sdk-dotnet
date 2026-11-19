---
title: "StreamObjectOptions<T>"
description: "Options for CancellationToken). Maps to streamObject."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamObjectOptions<T>"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class StreamObjectOptions<T>
```


Options for `CancellationToken)`. Maps to `streamObject`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` | The result: the object, the array, or the enum value (a string). |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L35)

## Properties

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_AllowSystemInMessages_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_AllowSystemInMessages"></a>

### AllowSystemInMessages


```csharp
public bool AllowSystemInMessages { get; set; }
```


Whether system messages are allowed in `Messages`. Default: &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;, which throws an
[InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/) for a system message; use `Instructions` instead. Maps to `allowSystemInMessages`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L62)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_EnumValues_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_EnumValues"></a>

### EnumValues


```csharp
public IReadOnlyList<string>? EnumValues { get; set; }
```


The enum values that the model should use. Only for enum output. Maps to `enum`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L88)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_ExperimentalDownload_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_ExperimentalDownload"></a>

### ExperimentalDownload


```csharp
public Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? ExperimentalDownload { get; set; }
```


Custom download function to use for URLs. By default, files are downloaded if the model does not support the URL for the
given media type. The function receives one [DownloadRequestItem](/api/Vercel.AI.Util/DownloadRequestItem/) per prompt URL, with
[IsUrlSupportedByModel](/api/Vercel.AI.Util/DownloadRequestItem/#Vercel_AI_Util_DownloadRequestItem_IsUrlSupportedByModel) set, and returns `null` for a URL the model can read itself.
Maps to `experimental_download`.

**Value:** [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L70)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_FrequencyPenalty_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L109)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Headers_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra headers forwarded to the model.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L121)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Instructions_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


Instructions supplied before the prompt.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L50)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_MaxOutputTokens_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L94)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_MaxRetries_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; set; }
```


Retries for provider calls made by this request.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L118)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Messages_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Full prompt messages.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L56)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Model_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit language model. Wins over `ModelId`.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L41)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_ModelId_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id such as `openai/gpt-4.1-mini`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L38)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnError_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnError"></a>

### OnError


```csharp
public Func<Exception, CancellationToken, Task>? OnError { get; set; }
```


Called when an error occurs during streaming. You can use it to log errors. The stream processing pauses until the
callback task completes.

**Value:** [Func&lt;Exception, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L146)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnFinish_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnFinish"></a>

### OnFinish


```csharp
public Func<GenerateObjectEndEvent<T>, CancellationToken, Task>? OnFinish { get; set; }
```


Called when the LLM response and the final object validation are finished.

**Value:** [Func&lt;GenerateObjectEndEvent&lt;T&gt;, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L149)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnStart_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnStart"></a>

### OnStart


```csharp
public Func<GenerateObjectStartEvent, CancellationToken, Task>? OnStart { get; set; }
```


Called when the streamObject operation begins, before the LLM call is made.

**Value:** [Func&lt;GenerateObjectStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L134)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnStepEnd_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called when the model streaming step completes, with the raw accumulated text before final schema validation.

**Value:** [Func&lt;GenerateObjectStepEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L140)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnStepStart_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_OnStepStart"></a>

### OnStepStart


```csharp
public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }
```


Called when the model call (step) begins, before the provider is called.

**Value:** [Func&lt;GenerateObjectStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L137)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Output_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Output"></a>

### Output


```csharp
public StreamObjectOutput Output { get; set; }
```


The type of the output. Default: [Object](/api/Vercel.AI.GenerateObject/StreamObjectOutput/#Vercel_AI_GenerateObject_StreamObjectOutput_Object).

**Value:** [StreamObjectOutput](/api/Vercel.AI.GenerateObject/StreamObjectOutput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L44)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_PresencePenalty_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L106)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Prompt_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; set; }
```


A single user prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L53)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L124)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Reasoning_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L115)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_RepairText_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_RepairText"></a>

### RepairText


```csharp
public Func<GenerateObjectRepairContext, CancellationToken, Task<string?>>? RepairText { get; set; }
```


Attempts to repair text that failed JSON parsing or schema validation.

**Value:** [Func&lt;GenerateObjectRepairContext, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L127)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Schema_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Schema"></a>

### Schema


```csharp
public string? Schema { get; set; }
```


JSON Schema for the generated object. For array output it is the schema of one element. Not supported for enum and
no-schema output.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L79)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_SchemaDescription_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_SchemaDescription"></a>

### SchemaDescription


```csharp
public string? SchemaDescription { get; set; }
```


Optional schema description forwarded to the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L85)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_SchemaName_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_SchemaName"></a>

### SchemaName


```csharp
public string? SchemaName { get; set; }
```


Optional schema name forwarded to the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L82)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Seed_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Random seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L112)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_SerializerOptions_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_SerializerOptions"></a>

### SerializerOptions


```csharp
public JsonSerializerOptions? SerializerOptions { get; set; }
```


Serializer settings used to materialize &lt;code class="typeparamref"&gt;T&lt;/code&gt;.

**Value:** [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L91)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_System_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_System"></a>

### System


```csharp
public string? System { get; set; }
```


System prompt. `Instructions` wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L47)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Temperature_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L97)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_TopK_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Top-k sampling.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L103)

<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_TopP_"></a>
<a id="Vercel_AI_GenerateObject_StreamObjectOptions_1_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Top-p sampling.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L100)
