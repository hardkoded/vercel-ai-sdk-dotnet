---
title: "AnthropicInlineToolConverter"
description: "Converts a tool that a mid-conversation tool change adds by value (a function or provider tool object) into its Anthropic definition. Maps to AnthropicInline..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicInlineToolConverter"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicInlineToolConverter"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public delegate AnthropicInlineToolResult AnthropicInlineToolConverter(JsonElement tool)
```


Converts a tool that a mid-conversation tool change adds by value (a `function` or `provider`
tool object) into its Anthropic definition. Maps to `AnthropicInlineToolConverter`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tool` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [AnthropicInlineToolResult](/api/Vercel.AI.Anthropic/AnthropicInlineToolResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L66)
