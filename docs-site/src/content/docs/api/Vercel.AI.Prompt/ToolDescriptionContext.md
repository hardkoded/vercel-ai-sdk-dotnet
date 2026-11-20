---
title: "ToolDescriptionContext"
description: "Context passed to a function-valued tool description."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolDescriptionContext"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolDescriptionContext"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ToolDescriptionContext
```


Context passed to a function-valued tool description.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L11)

## Constructors

<a id="Vercel_AI_Prompt_ToolDescriptionContext__ctor_"></a>
<a id="Vercel_AI_Prompt_ToolDescriptionContext__ctor_System_Object_Vercel_AI_ProviderUtils_ISandboxSession_"></a>

### ToolDescriptionContext(object?, ISandboxSession?)


```csharp
public ToolDescriptionContext(object? context, ISandboxSession? experimentalSandbox)
```


Creates context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `context` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `experimentalSandbox` | [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L14)

## Properties

<a id="Vercel_AI_Prompt_ToolDescriptionContext_Context_"></a>
<a id="Vercel_AI_Prompt_ToolDescriptionContext_Context"></a>

### Context


```csharp
public object? Context { get; }
```


Per-tool context value.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L21)

<a id="Vercel_AI_Prompt_ToolDescriptionContext_ExperimentalSandbox_"></a>
<a id="Vercel_AI_Prompt_ToolDescriptionContext_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; }
```


Sandbox session, when a sandbox is active. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L24)
