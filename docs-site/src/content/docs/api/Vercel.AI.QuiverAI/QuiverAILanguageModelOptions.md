---
title: "QuiverAILanguageModelOptions"
description: "The quiverai provider options of a language model call. Maps to QuiverAILanguageModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAILanguageModelOptions"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAILanguageModelOptions
```


The `quiverai` provider options of a language model call. Maps to `QuiverAILanguageModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAILanguageModelOptions.cs#L34)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public QuiverAIReasoningEffort? ReasoningEffort { get; set; }
```


Controls the amount of reasoning used by the model.

**Value:** `QuiverAIReasoningEffort?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAILanguageModelOptions.cs#L37)

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions_ReasoningSummary_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions_ReasoningSummary"></a>

### ReasoningSummary


```csharp
public QuiverAIReasoningSummary? ReasoningSummary { get; set; }
```


Requests the model's safe reasoning summary.

**Value:** `QuiverAIReasoningSummary?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAILanguageModelOptions.cs#L40)

## Methods

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "quiverai")
```


Returns the options as call provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;
(`quiverai` by default), ready for `LanguageModelCallOptions.ProviderOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAILanguageModelOptions.cs#L46)
