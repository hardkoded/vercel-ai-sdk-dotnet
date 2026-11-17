---
title: "OpenAIShellAction"
description: "The shell action of a OpenAIShellInput. Maps to the action of the upstream shell input."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellAction"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellAction"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellAction
```


The shell action of a [OpenAIShellInput](/api/Vercel.AI.OpenAI/OpenAIShellInput/). Maps to the `action` of the upstream shell input.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L12)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellAction_Commands_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellAction_Commands"></a>

### Commands


```csharp
public IReadOnlyList<string> Commands { get; set; }
```


A list of shell commands to execute.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L15)

<a id="Vercel_AI_OpenAI_OpenAIShellAction_MaxOutputLength_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellAction_MaxOutputLength"></a>

### MaxOutputLength


```csharp
public double? MaxOutputLength { get; set; }
```


Optional maximum number of characters to return from each command.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L21)

<a id="Vercel_AI_OpenAI_OpenAIShellAction_TimeoutMs_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellAction_TimeoutMs"></a>

### TimeoutMs


```csharp
public double? TimeoutMs { get; set; }
```


Optional timeout in milliseconds for the commands.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L18)
