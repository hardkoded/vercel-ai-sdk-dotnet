---
title: "CerebrasLanguageModelChatOptions"
description: "The cerebras provider options of a chat call. Maps to CerebrasLanguageModelChatOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasLanguageModelChatOptions"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public sealed class CerebrasLanguageModelChatOptions
```


The `cerebras` provider options of a chat call. Maps to `CerebrasLanguageModelChatOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L74)

## Properties

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_LogitBias_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_LogitBias"></a>

### LogitBias


```csharp
public IReadOnlyDictionary<string, double>? LogitBias { get; set; }
```


Bias token selection by token ID (each bias from -100 to 100).

**Value:** [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L92)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_Logprobs_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_Logprobs"></a>

### Logprobs


```csharp
public bool? Logprobs { get; set; }
```


Return token log probabilities.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L86)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ParallelToolCalls_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ParallelToolCalls"></a>

### ParallelToolCalls


```csharp
public bool? ParallelToolCalls { get; set; }
```


Allow parallel function calls.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L83)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_Prediction_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_Prediction"></a>

### Prediction


```csharp
public CerebrasPrediction? Prediction { get; set; }
```


Predicted output content.

**Value:** [CerebrasPrediction](/api/Vercel.AI.Cerebras/CerebrasPrediction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L107)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_PromptCacheKey_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_PromptCacheKey"></a>

### PromptCacheKey


```csharp
public string? PromptCacheKey { get; set; }
```


Cache affinity key for shared prompts (at most 1024 characters).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L110)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_QueueThreshold_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_QueueThreshold"></a>

### QueueThreshold


```csharp
public int? QueueThreshold { get; set; }
```


Maximum queue time in milliseconds for flex/auto requests (50 to 20000).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L98)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public CerebrasReasoningEffort? ReasoningEffort { get; set; }
```


Reasoning effort for configurable models.

**Value:** `CerebrasReasoningEffort?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L101)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ReasoningFormat_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ReasoningFormat"></a>

### ReasoningFormat


```csharp
public CerebrasReasoningFormat? ReasoningFormat { get; set; }
```


Format of the returned reasoning.

**Value:** `CerebrasReasoningFormat?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L104)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ServiceTier_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ServiceTier"></a>

### ServiceTier


```csharp
public CerebrasServiceTier? ServiceTier { get; set; }
```


Request processing tier.

**Value:** `CerebrasServiceTier?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L95)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_StrictJsonSchema_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_StrictJsonSchema"></a>

### StrictJsonSchema


```csharp
public bool? StrictJsonSchema { get; set; }
```


Use strict JSON schema validation.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L80)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_TopLogprobs_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_TopLogprobs"></a>

### TopLogprobs


```csharp
public int? TopLogprobs { get; set; }
```


Number of token log probabilities to return (0 to 20).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L89)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_User_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_User"></a>

### User


```csharp
public string? User { get; set; }
```


End-user identifier for abuse monitoring.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L77)

## Methods

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelChatOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "cerebras")
```


Validates the options and returns them as call provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;
(`cerebras` by default), ready for `LanguageModelCallOptions.ProviderOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong value. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L117)
