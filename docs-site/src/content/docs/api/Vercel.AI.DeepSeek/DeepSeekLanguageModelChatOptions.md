---
title: "DeepSeekLanguageModelChatOptions"
description: "The deepseek provider options of a chat call. Maps to DeepSeekLanguageModelChatOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekLanguageModelChatOptions"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekLanguageModelChatOptions
```


The `deepseek` provider options of a chat call. Maps to `DeepSeekLanguageModelChatOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L13)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_Logprobs_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_Logprobs"></a>

### Logprobs


```csharp
public bool? Logprobs { get; set; }
```


Whether to return log probabilities for generated tokens.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L24)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public string? ReasoningEffort { get; set; }
```


Controls the thinking strength for DeepSeek V4 reasoning models: `low`, `high` or `max`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L42)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_StrictJsonSchema_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_StrictJsonSchema"></a>

### StrictJsonSchema


```csharp
public bool? StrictJsonSchema { get; set; }
```


Whether to use strict JSON schema validation for structured outputs. Only applies when the serving endpoint supports
JSON schema response formats (for example Azure). Defaults to &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;true&lt;/a&gt;.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L48)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_Thinking_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_Thinking"></a>

### Thinking


```csharp
public DeepSeekThinkingOptions? Thinking { get; set; }
```


Controls whether thinking mode is enabled. Thinking is enabled by default.

**Value:** [DeepSeekThinkingOptions](/api/Vercel.AI.DeepSeek/DeepSeekThinkingOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L39)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_TopLogprobs_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_TopLogprobs"></a>

### TopLogprobs


```csharp
public int? TopLogprobs { get; set; }
```


Number of most likely tokens to return at each token position (0 to 20). Setting this option automatically enables
[Logprobs](#Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_Logprobs).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L30)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_UserId_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_UserId"></a>

### UserId


```csharp
public string? UserId { get; set; }
```


An opaque identifier for the end user. DeepSeek uses it for content-safety tracing and request isolation.
It must contain only ASCII letters, numbers, underscores, and hyphens, and be at most 512 characters long.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L36)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelChatOptions_ToProviderOptions_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L55)
