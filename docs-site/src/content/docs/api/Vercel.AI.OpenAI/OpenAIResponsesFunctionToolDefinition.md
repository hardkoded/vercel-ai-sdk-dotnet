---
title: "OpenAIResponsesFunctionToolDefinition"
description: "A function tool definition (type: 'function')."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesFunctionToolDefinition"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesFunctionToolDefinition : OpenAIResponsesAdditionalTool
```


A function tool definition (`type: 'function'`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAIResponsesAdditionalTool](/api/Vercel.AI.OpenAI/OpenAIResponsesAdditionalTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L26)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_DeferLoading_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_DeferLoading"></a>

### DeferLoading


```csharp
public bool? DeferLoading { get; set; }
```


Defers loading the tool (`defer_loading`).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L38)

<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


The tool description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L29)

<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_Parameters_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_Parameters"></a>

### Parameters


```csharp
public JsonElement Parameters { get; set; }
```


The JSON Schema of the tool input, an object.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L32)

<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_Strict_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesFunctionToolDefinition_Strict"></a>

### Strict


```csharp
public bool? Strict { get; set; }
```


Strict mode.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L35)
