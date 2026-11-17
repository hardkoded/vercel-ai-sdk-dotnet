---
title: "DeepSeekLanguageModelResponsesOptions"
description: "The deepseek provider options of a Responses call. Maps to DeepSeekLanguageModelResponsesOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekLanguageModelResponsesOptions"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekLanguageModelResponsesOptions
```


The `deepseek` provider options of a Responses call. Maps to `DeepSeekLanguageModelResponsesOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekLanguageModelResponsesOptions.cs#L12)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public string? ReasoningEffort { get; set; }
```


Controls the thinking strength when thinking is enabled: `low`, `high` or `max`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekLanguageModelResponsesOptions.cs#L26)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_Thinking_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_Thinking"></a>

### Thinking


```csharp
public DeepSeekThinkingOptions? Thinking { get; set; }
```


Controls whether thinking mode is enabled. Thinking is enabled by default.

**Value:** [DeepSeekThinkingOptions](/api/Vercel.AI.DeepSeek/DeepSeekThinkingOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekLanguageModelResponsesOptions.cs#L23)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_TopLogprobs_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_TopLogprobs"></a>

### TopLogprobs


```csharp
public int? TopLogprobs { get; set; }
```


Number of most likely tokens to return at each token position (0 to 20).
Log probabilities are returned in `providerMetadata.deepseek.logprobs`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekLanguageModelResponsesOptions.cs#L32)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_UserId_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_UserId"></a>

### UserId


```csharp
public string? UserId { get; set; }
```


An opaque identifier for the end user. DeepSeek uses it for content-safety tracing and request isolation.
It must contain only ASCII letters, numbers, underscores, and hyphens, and be at most 512 characters long.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekLanguageModelResponsesOptions.cs#L38)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelResponsesOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "deepseek")
```


Validates the options and returns them as call provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;
(`deepseek` by default), ready for `LanguageModelCallOptions.ProviderOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | An option has the wrong value. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekLanguageModelResponsesOptions.cs#L45)
