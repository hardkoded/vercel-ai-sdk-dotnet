---
title: "OpenAIShellOutcome"
description: "The outcome of one shell command: timeout, or exit with ExitCode."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellOutcome"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellOutcome"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellOutcome
```


The outcome of one shell command: `timeout`, or `exit` with [ExitCode](#Vercel_AI_OpenAI_OpenAIShellOutcome_ExitCode).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L32)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellOutcome_ExitCode_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutcome_ExitCode"></a>

### ExitCode


```csharp
public double? ExitCode { get; set; }
```


Exit code of an `exit` outcome.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L38)

<a id="Vercel_AI_OpenAI_OpenAIShellOutcome_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellOutcome_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


`timeout` or `exit`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L35)
