---
title: "OpenAIShellToolOptions"
description: "Options of Shell(OpenAIShellToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellToolOptions
```


Options of [Shell(OpenAIShellToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_Shell_Vercel_AI_OpenAI_OpenAIShellToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L231)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_Environment_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_Environment"></a>

### Environment


```csharp
public OpenAIShellEnvironment? Environment { get; set; }
```


The shell environment.

**Value:** [OpenAIShellEnvironment](/api/Vercel.AI.OpenAI/OpenAIShellEnvironment/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L234)

<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_Execute_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_Execute"></a>

### Execute


```csharp
public Func<OpenAIShellInput, ToolExecutionOptions, Task<OpenAIShellOutput>>? Execute { get; set; }
```


Executes the shell commands of a local call and returns their output. Maps to `execute`.

**Value:** [Func&lt;OpenAIShellInput, ToolExecutionOptions, Task&lt;OpenAIShellOutput&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L237)

<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_NeedsApproval_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_NeedsApproval"></a>

### NeedsApproval


```csharp
public bool NeedsApproval { get; set; }
```


When true, a call needs approval before it runs. Maps to `needsApproval: true`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L244)

<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_NeedsApprovalFunction_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellToolOptions_NeedsApprovalFunction"></a>

### NeedsApprovalFunction


```csharp
public Func<OpenAIShellInput, ToolNeedsApprovalOptions, Task<bool>>? NeedsApprovalFunction { get; set; }
```


Decides for each call whether it needs approval. Used instead of [NeedsApproval](#Vercel_AI_OpenAI_OpenAIShellToolOptions_NeedsApproval). Maps to the function form of `needsApproval`.

**Value:** [Func&lt;OpenAIShellInput, ToolNeedsApprovalOptions, Task&lt;bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L247)
