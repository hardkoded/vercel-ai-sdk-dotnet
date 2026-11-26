---
title: "GenerateObjectStartEvent"
description: "Event sent before object generation begins."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectStartEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectStartEvent : GenerateObjectEvent, IOperationStartEvent
```


Event sent before object generation begins.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/)

**Implements:** [IOperationStartEvent](/api/Vercel.AI/IOperationStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L301)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_FrequencyPenalty_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; init; }
```


Frequency penalty. Maps to `frequencyPenalty`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L344)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Headers_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; init; }
```


Additional HTTP headers sent with the request. Maps to `headers`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L353)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_MaxOutputTokens_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; init; }
```


Maximum number of tokens to generate. Maps to `maxOutputTokens`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L329)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; init; }
```


Maximum number of retries for failed requests. Maps to `maxRetries`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L350)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Messages_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; }
```


The prompt messages, when supplied.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L320)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_OperationId_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; init; }
```


Identifies the operation type, `ai.generateObject` or `ai.streamObject`. Maps to `operationId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L323)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Output_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Output"></a>

### Output


```csharp
public string Output { get; init; }
```


The output strategy type: `object`, `array`, `enum` or `no-schema`. Maps to `output`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L377)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_PresencePenalty_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; init; }
```


Presence penalty. Maps to `presencePenalty`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L341)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Prompt_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; }
```


The simple prompt, when supplied.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L317)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; }
```


Additional provider-specific options. Maps to `providerOptions`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L356)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Schema_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Schema"></a>

### Schema


```csharp
public JsonElement? Schema { get; }
```


The JSON Schema used for object generation, if any. Maps to `schema`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L380)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_SchemaDescription_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_SchemaDescription"></a>

### SchemaDescription


```csharp
public string? SchemaDescription { get; }
```


Optional description of the schema. Maps to `schemaDescription`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L386)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_SchemaName_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_SchemaName"></a>

### SchemaName


```csharp
public string? SchemaName { get; }
```


Optional name of the schema. Maps to `schemaName`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L383)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Seed_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Seed"></a>

### Seed


```csharp
public int? Seed { get; init; }
```


Random seed. Maps to `seed`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L347)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_System_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_System"></a>

### System


```csharp
public Instructions? System { get; }
```


The system message(s) provided to the model. Maps to `system`.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L326)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Temperature_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; init; }
```


Sampling temperature. Maps to `temperature`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L332)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_TopK_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_TopK"></a>

### TopK


```csharp
public int? TopK { get; init; }
```


Top-k sampling parameter. Maps to `topK`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L338)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_TopP_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_TopP"></a>

### TopP


```csharp
public double? TopP { get; init; }
```


Top-p sampling parameter. Maps to `topP`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L335)
