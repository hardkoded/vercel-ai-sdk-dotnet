---
title: "OpenAIResponsesNamespaceToolDefinition"
description: "A namespace of function tools (type: 'namespace')."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesNamespaceToolDefinition"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesNamespaceToolDefinition"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesNamespaceToolDefinition : OpenAIResponsesAdditionalTool
```


A namespace of function tools (`type: 'namespace'`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAIResponsesAdditionalTool](/api/Vercel.AI.OpenAI/OpenAIResponsesAdditionalTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L115)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesNamespaceToolDefinition_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesNamespaceToolDefinition_Description"></a>

### Description


```csharp
public string Description { get; set; }
```


The namespace description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L118)

<a id="Vercel_AI_OpenAI_OpenAIResponsesNamespaceToolDefinition_Tools_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesNamespaceToolDefinition_Tools"></a>

### Tools


```csharp
public IReadOnlyList<OpenAIResponsesFunctionToolDefinition> Tools { get; set; }
```


The function tools of the namespace. At least one.

**Value:** [IReadOnlyList&lt;OpenAIResponsesFunctionToolDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L121)
