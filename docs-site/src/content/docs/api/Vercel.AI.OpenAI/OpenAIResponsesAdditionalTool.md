---
title: "OpenAIResponsesAdditionalTool"
description: "A single OpenAI-format tool definition that can be loaded at a position in the conversation. History is replayable, so the provider treats these as reference..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesAdditionalTool"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesAdditionalTool"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public abstract class OpenAIResponsesAdditionalTool
```


A single OpenAI-format tool definition that can be loaded at a position in the conversation. History is
replayable, so the provider treats these as references to tools registered for the current request and sends its
own definition for the registered tool. Maps to `OpenAIResponsesAdditionalTool`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [OpenAIResponsesCustomToolDefinition](/api/Vercel.AI.OpenAI/OpenAIResponsesCustomToolDefinition/), [OpenAIResponsesFunctionToolDefinition](/api/Vercel.AI.OpenAI/OpenAIResponsesFunctionToolDefinition/), [OpenAIResponsesNamespaceToolDefinition](/api/Vercel.AI.OpenAI/OpenAIResponsesNamespaceToolDefinition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L15)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesAdditionalTool_Name_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesAdditionalTool_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


The tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L20)
