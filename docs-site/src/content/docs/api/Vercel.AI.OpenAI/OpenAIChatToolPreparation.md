---
title: "OpenAIChatToolPreparation"
description: "Chat Completions tools and tool choice."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatToolPreparation"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIChatToolPreparation
```


Chat Completions tools and tool choice.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L28)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation_ToolChoice_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation_ToolChoice"></a>

### ToolChoice


```csharp
public JsonNode? ToolChoice { get; }
```


OpenAI `tool_choice`. Null when the caller did not set one.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L41)

<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation_Tools_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation_Tools"></a>

### Tools


```csharp
public JsonArray? Tools { get; }
```


Tool objects. Null when the caller passed no tools.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L38)

<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatToolPreparation_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing tools.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L44)
