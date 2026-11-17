---
title: "OpenAIShellOutputItem"
description: "The output of one shell command."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellOutputItem"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellOutputItem
```


The output of one shell command.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L42)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem_Outcome_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem_Outcome"></a>

### Outcome


```csharp
public OpenAIShellOutcome Outcome { get; set; }
```


The outcome of the shell execution.

**Value:** [OpenAIShellOutcome](/api/Vercel.AI.OpenAI/OpenAIShellOutcome/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L51)

<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem_Stderr_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem_Stderr"></a>

### Stderr


```csharp
public string Stderr { get; set; }
```


Standard error from the command.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L48)

<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem_Stdout_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutputItem_Stdout"></a>

### Stdout


```csharp
public string Stdout { get; set; }
```


Standard output from the command.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L45)
