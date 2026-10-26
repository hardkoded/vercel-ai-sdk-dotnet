---
title: "AnthropicPrompt"
description: "Converts SDK prompt messages into Anthropic message blocks."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicPrompt"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicPrompt"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicPrompt
```


Converts SDK prompt messages into Anthropic message blocks.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L39)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicPrompt_Convert_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPrompt_Convert_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Boolean_Vercel_AI_Anthropic_AnthropicCacheControlValidator_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__Vercel_AI_Anthropic_AnthropicMediaPartConverter_"></a>

### `Convert(IReadOnlyList<ModelMessage>, bool, AnthropicCacheControlValidator?, IReadOnlyDictionary<string, string>?, IReadOnlyDictionary<string, string>?, AnthropicMediaPartConverter?)`


```csharp
public static AnthropicPromptResult Convert(IReadOnlyList<ModelMessage> prompt, bool sendReasoning = true, AnthropicCacheControlValidator? cacheControl = null, IReadOnlyDictionary<string, string>? providerToolNames = null, IReadOnlyDictionary<string, string>? toolsetNames = null, AnthropicMediaPartConverter? convertMediaPart = null)
```


Converts SDK prompt messages. Maps to `convertToAnthropicPrompt`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `sendReasoning` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cacheControl` | [AnthropicCacheControlValidator](/api/Vercel.AI.Anthropic/AnthropicCacheControlValidator/) |  |
| `providerToolNames` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `toolsetNames` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `convertMediaPart` | [AnthropicMediaPartConverter](/api/Vercel.AI.Anthropic/AnthropicMediaPartConverter/) |  |

**Returns:** [AnthropicPromptResult](/api/Vercel.AI.Anthropic/AnthropicPromptResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L42)
