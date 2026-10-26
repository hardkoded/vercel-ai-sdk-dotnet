---
title: "LanguageModelCallOptions"
description: "Settings for one language-model call. Property names follow the V4 call options."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelCallOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelCallOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelCallOptions
```


Settings for one language-model call. Property names follow the V4 call options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1457)

## Properties

<a id="Vercel_AI_Provider_LanguageModelCallOptions_AbortSignal_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_AbortSignal"></a>

### AbortSignal


```csharp
public AbortSignal? AbortSignal { get; set; }
```


Aborts when the call is aborted. It carries the reason, for example a [TimeoutError](/api/Vercel.AI.Util/TimeoutError/) for an expired timeout.
Set only when the call has a timeout or a [AbortSignal](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_AbortSignal). The [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) of the call is cancelled at the same time. Maps to `abortSignal`.

**Value:** [AbortSignal](/api/Vercel.AI.Util/AbortSignal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1522)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_FrequencyPenalty_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1478)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_Headers_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra HTTP headers for this call.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1505)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_IncludeRawChunks_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool IncludeRawChunks { get; set; }
```


When true, the stream includes one raw provider chunk for each event.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1516)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonResponse_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonResponse"></a>

### JsonResponse


```csharp
public bool JsonResponse { get; set; }
```


Asks for a JSON response without a schema. Maps to `responseFormat: { type: 'json' }` with no schema. Providers send `json_object` or the matching format. Ignored when [JsonSchema](#Vercel_AI_Provider_LanguageModelCallOptions_JsonSchema) is set.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1487)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonSchema_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonSchema"></a>

### JsonSchema


```csharp
public JsonElement? JsonSchema { get; set; }
```


JSON schema when structured output is requested.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1490)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonSchemaDescription_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonSchemaDescription"></a>

### JsonSchemaDescription


```csharp
public string? JsonSchemaDescription { get; set; }
```


Response format description. Maps to `responseFormat.description`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1496)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonSchemaName_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_JsonSchemaName"></a>

### JsonSchemaName


```csharp
public string? JsonSchemaName { get; set; }
```


Schema name sent to providers that require one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1493)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum tokens to generate. Maps to `maxOutputTokens`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1463)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_PresencePenalty_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1475)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_Prompt_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_Prompt"></a>

### Prompt


```csharp
public IReadOnlyList<ModelMessage> Prompt { get; set; }
```


Prompt messages.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1460)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider-specific options, keyed by provider name.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1508)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_Reasoning_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort for providers that accept it: `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, or `max`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1513)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_Seed_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Random seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1484)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_StopSequences_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; set; }
```


Stop sequences.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1481)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_Temperature_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Sampling temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1466)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_ToolChoice_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; set; }
```


Tool selection mode.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1502)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_Tools_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<LanguageModelTool>? Tools { get; set; }
```


Tools the model may call.

**Value:** [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1499)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_TopK_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Top-k sampling.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1472)

<a id="Vercel_AI_Provider_LanguageModelCallOptions_TopP_"></a>
<a id="Vercel_AI_Provider_LanguageModelCallOptions_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Nucleus sampling.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1469)
