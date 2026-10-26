---
title: "AnthropicPromptResult"
description: "Anthropic system and messages produced from an SDK prompt."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicPromptResult"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicPromptResult"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicPromptResult
```


Anthropic `system` and `messages` produced from an SDK prompt.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L14)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicPromptResult__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPromptResult__ctor_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonArray_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicWarning__"></a>

### `AnthropicPromptResult(JsonArray?, JsonArray, IReadOnlyList<string>, IReadOnlyList<AnthropicWarning>)`


```csharp
public AnthropicPromptResult(JsonArray? system, JsonArray messages, IReadOnlyList<string> betas, IReadOnlyList<AnthropicWarning> warnings)
```


Creates a converted prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `system` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `messages` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L17)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicPromptResult_Betas_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPromptResult_Betas"></a>

### Betas


```csharp
public IReadOnlyList<string> Betas { get; }
```


Betas required by the prompt.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L32)

<a id="Vercel_AI_Anthropic_AnthropicPromptResult_Messages_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPromptResult_Messages"></a>

### Messages


```csharp
public JsonArray Messages { get; }
```


Conversation messages.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L29)

<a id="Vercel_AI_Anthropic_AnthropicPromptResult_System_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPromptResult_System"></a>

### System


```csharp
public JsonArray? System { get; }
```


Top-level system blocks. Null when the prompt has no system text.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L26)

<a id="Vercel_AI_Anthropic_AnthropicPromptResult_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPromptResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<AnthropicWarning> Warnings { get; }
```


Warnings produced while converting.

**Value:** [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L35)
