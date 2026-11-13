---
title: "AnthropicInlineToolResult"
description: "The Anthropic form of a tool that a mid-conversation tool change defines by value."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicInlineToolResult"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicInlineToolResult
```


The Anthropic form of a tool that a mid-conversation tool change defines by value.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L42)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicWarning__System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `AnthropicInlineToolResult(JsonObject?, IReadOnlyList<AnthropicWarning>?, IReadOnlyList<string>?)`


```csharp
public AnthropicInlineToolResult(JsonObject? definition, IReadOnlyList<AnthropicWarning>? warnings = null, IReadOnlyList<string>? betas = null)
```


Creates a converted inline tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `definition` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L45)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult_Betas_"></a>
<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult_Betas"></a>

### Betas


```csharp
public IReadOnlyList<string> Betas { get; }
```


Betas the tool requires.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L59)

<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult_Definition_"></a>
<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult_Definition"></a>

### Definition


```csharp
public JsonObject? Definition { get; }
```


The tool definition. Null when the tool is dropped.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L53)

<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicInlineToolResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<AnthropicWarning> Warnings { get; }
```


Warnings from converting the tool.

**Value:** [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicPrompt.cs#L56)
