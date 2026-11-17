---
title: "OpenAIApplyPatchOperation"
description: "A patch operation of the applypatch tool: createfile, deletefile, or updatefile."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIApplyPatchOperation"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIApplyPatchOperation
```


A patch operation of the apply_patch tool: `create_file`, `delete_file`, or `update_file`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L65)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation_Diff_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation_Diff"></a>

### Diff


```csharp
public string? Diff { get; set; }
```


Unified diff content of a `create_file` or `update_file` operation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L74)

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation_Path_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation_Path"></a>

### Path


```csharp
public string Path { get; set; }
```


Path of the file relative to the workspace root.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L71)

<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIApplyPatchOperation_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


`create_file`, `delete_file`, or `update_file`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIClientTools.cs#L68)
