---
title: "OpenAIResponsesCustomToolDefinition"
description: "A custom tool definition (type: 'custom')."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesCustomToolDefinition"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolDefinition"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesCustomToolDefinition : OpenAIResponsesAdditionalTool
```


A custom tool definition (`type: 'custom'`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAIResponsesAdditionalTool](/api/Vercel.AI.OpenAI/OpenAIResponsesAdditionalTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L64)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolDefinition_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolDefinition_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


The tool description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L67)

<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolDefinition_Format_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolDefinition_Format"></a>

### Format


```csharp
public OpenAIResponsesCustomToolFormat? Format { get; set; }
```


The input format. `null` leaves it to the provider.

**Value:** [OpenAIResponsesCustomToolFormat](/api/Vercel.AI.OpenAI/OpenAIResponsesCustomToolFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L70)
