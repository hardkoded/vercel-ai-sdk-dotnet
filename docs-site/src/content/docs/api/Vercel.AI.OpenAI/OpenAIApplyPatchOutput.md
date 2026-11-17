---
title: "OpenAIApplyPatchOutput"
description: "The output of the applypatch tool, what the application sends back."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIApplyPatchOutput"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOutput"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIApplyPatchOutput
```


The output of the apply_patch tool, what the application sends back.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L88)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOutput_Output_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOutput_Output"></a>

### Output


```csharp
public string? Output { get; set; }
```


Optional human-readable log text, such as patch results or errors.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L94)

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOutput_Status_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOutput_Status"></a>

### Status


```csharp
public string Status { get; set; }
```


`completed` when the patch was applied, `failed` when it was not.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L91)
