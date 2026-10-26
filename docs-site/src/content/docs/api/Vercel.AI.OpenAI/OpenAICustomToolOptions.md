---
title: "OpenAICustomToolOptions"
description: "Options of CustomTool(OpenAICustomToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICustomToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAICustomToolOptions
```


Options of [CustomTool(OpenAICustomToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_CustomTool_Vercel_AI_OpenAI_OpenAICustomToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L204)

## Properties

<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


Description of the tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L210)

<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_GrammarDefinition_"></a>
<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_GrammarDefinition"></a>

### GrammarDefinition


```csharp
public string? GrammarDefinition { get; set; }
```


Grammar definition, used with [GrammarSyntax](#Vercel_AI_OpenAI_OpenAICustomToolOptions_GrammarSyntax).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L216)

<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_GrammarSyntax_"></a>
<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_GrammarSyntax"></a>

### GrammarSyntax


```csharp
public string? GrammarSyntax { get; set; }
```


Grammar syntax of the input: `regex` or `lark`. Without it the input is unconstrained text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L213)

<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_Name_"></a>
<a id="Vercel_AI_OpenAI_OpenAICustomToolOptions_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


The tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L207)
