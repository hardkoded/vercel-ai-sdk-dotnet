---
title: "OpenAIApplyPatchToolOptions"
description: "Options of ApplyPatch(OpenAIApplyPatchToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIApplyPatchToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIApplyPatchToolOptions
```


Options of [ApplyPatch(OpenAIApplyPatchToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_ApplyPatch_Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L255)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_Execute_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_Execute"></a>

### Execute


```csharp
public Func<OpenAIApplyPatchInput, ToolExecutionOptions, Task<OpenAIApplyPatchOutput>>? Execute { get; set; }
```


Applies the patch operation of a call and returns its status. Maps to `execute`.

**Value:** [Func&lt;OpenAIApplyPatchInput, ToolExecutionOptions, Task&lt;OpenAIApplyPatchOutput&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L258)

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_NeedsApproval_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_NeedsApproval"></a>

### NeedsApproval


```csharp
public bool NeedsApproval { get; set; }
```


When true, a call needs approval before it runs. Maps to `needsApproval: true`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L265)

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_NeedsApprovalFunction_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_NeedsApprovalFunction"></a>

### NeedsApprovalFunction


```csharp
public Func<OpenAIApplyPatchInput, ToolNeedsApprovalOptions, Task<bool>>? NeedsApprovalFunction { get; set; }
```


Decides for each call whether it needs approval. Used instead of [NeedsApproval](#Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_NeedsApproval). Maps to the function form of `needsApproval`.

**Value:** [Func&lt;OpenAIApplyPatchInput, ToolNeedsApprovalOptions, Task&lt;bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L268)
