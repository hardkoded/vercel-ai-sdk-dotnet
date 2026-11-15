---
title: "GenerateObjectOptions<T>"
description: "Options for CancellationToken). Maps to generateObject."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectOptions<T>"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectOptions<T>
```


Options for `CancellationToken)`. Maps to `generateObject`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L53)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_FrequencyPenalty_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L101)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Headers_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra headers forwarded to the model.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L116)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Instructions_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


Instructions supplied before the prompt.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L65)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_MaxOutputTokens_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L86)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_MaxRetries_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; set; }
```


Retries for provider calls made by this request.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L110)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Messages_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Full prompt messages.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L71)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Model_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit language model. Wins over `ModelId`.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L59)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_ModelId_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id such as `openai/gpt-4.1-mini`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L56)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnFinish_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnFinish"></a>

### OnFinish


```csharp
public Func<GenerateObjectEndEvent<T>, CancellationToken, Task>? OnFinish { get; set; }
```


Called after the object is parsed and validated.

**Value:** [Func&lt;GenerateObjectEndEvent&lt;T&gt;, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L134)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnStart_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnStart"></a>

### OnStart


```csharp
public Func<GenerateObjectStartEvent, CancellationToken, Task>? OnStart { get; set; }
```


Called before generation begins.

**Value:** [Func&lt;GenerateObjectStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L125)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnStepEnd_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called with the raw model result before the selected text is parsed.

**Value:** [Func&lt;GenerateObjectStepEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L131)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnStepStart_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_OnStepStart"></a>

### OnStepStart


```csharp
public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }
```


Called immediately before the model call.

**Value:** [Func&lt;GenerateObjectStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L128)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_PresencePenalty_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L98)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Prompt_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; set; }
```


A single user prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L68)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L119)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Reasoning_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L107)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_RepairText_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_RepairText"></a>

### RepairText


```csharp
public Func<GenerateObjectRepairContext, CancellationToken, Task<string?>>? RepairText { get; set; }
```


Attempts to repair text that failed JSON parsing or schema validation.

**Value:** [Func&lt;GenerateObjectRepairContext, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L122)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Schema_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Schema"></a>

### Schema


```csharp
public string Schema { get; set; }
```


JSON Schema for the generated object.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L74)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_SchemaDescription_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_SchemaDescription"></a>

### SchemaDescription


```csharp
public string? SchemaDescription { get; set; }
```


Optional schema description forwarded to the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L80)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_SchemaName_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_SchemaName"></a>

### SchemaName


```csharp
public string? SchemaName { get; set; }
```


Optional schema name forwarded to the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L77)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Seed_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Random seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L104)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_SerializerOptions_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_SerializerOptions"></a>

### SerializerOptions


```csharp
public JsonSerializerOptions? SerializerOptions { get; set; }
```


Serializer settings used to materialize &lt;code class="typeparamref"&gt;T&lt;/code&gt;.

**Value:** [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L83)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_System_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_System"></a>

### System


```csharp
public string? System { get; set; }
```


System prompt. `Instructions` wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L62)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Temperature_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L89)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Timeout_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


Total or per-step timeout.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L113)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_TopK_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Top-k sampling.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L95)

<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_TopP_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectOptions_1_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Top-p sampling.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L92)
