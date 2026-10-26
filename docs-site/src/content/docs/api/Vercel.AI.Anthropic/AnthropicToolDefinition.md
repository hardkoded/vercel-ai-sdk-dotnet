---
title: "AnthropicToolDefinition"
description: "A function or provider tool passed to AnthropicToolPreparer."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicToolDefinition"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicToolDefinition
```


A function or provider tool passed to [AnthropicToolPreparer](/api/Vercel.AI.Anthropic/AnthropicToolPreparer/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L11)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Args_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Args"></a>

### Args


```csharp
public JsonElement? Args { get; set; }
```


Provider tool arguments.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L32)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Description_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


Function description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L20)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Id_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


Provider tool id, such as `anthropic.web_search_20250305`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L29)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_InputExamples_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_InputExamples"></a>

### InputExamples


```csharp
public JsonElement? InputExamples { get; set; }
```


Example inputs. Each item is an object with an `input` property, or the input itself.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L38)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_InputSchema_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_InputSchema"></a>

### InputSchema


```csharp
public JsonElement? InputSchema { get; set; }
```


JSON Schema for a function tool.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L23)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Name_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


Tool name sent to the model for function tools.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L17)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_ProviderOptions_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options, including Anthropic cache control and tool options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L35)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Strict_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Strict"></a>

### Strict


```csharp
public bool? Strict { get; set; }
```


Strict structured-output flag.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L26)

<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Type_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolDefinition_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


`function` or `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L14)
