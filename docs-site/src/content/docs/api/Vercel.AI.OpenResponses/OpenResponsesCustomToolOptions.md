---
title: "OpenResponsesCustomToolOptions"
description: "Options of CustomTool(OpenResponsesCustomToolOptions). Maps to OpenResponsesCustomToolOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesCustomToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesCustomToolOptions
```


Options of [CustomTool(OpenResponsesCustomToolOptions)](/api/Vercel.AI.OpenResponses/OpenResponsesTools/#Vercel_AI_OpenResponses_OpenResponsesTools_CustomTool_Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_). Maps to `OpenResponsesCustomToolOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L27)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Description_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


An optional description of what the custom tool does.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L33)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Execute_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Execute"></a>

### Execute


```csharp
public Func<string, ToolExecutionOptions, Task<string>>? Execute { get; set; }
```


Runs a call with the raw string input and returns its output. Maps to `execute`.

**Value:** [Func&lt;string, ToolExecutionOptions, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L39)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Format_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Format"></a>

### Format


```csharp
public OpenResponsesCustomToolFormat? Format { get; set; }
```


The raw text format returned by the model.

**Value:** [OpenResponsesCustomToolFormat](/api/Vercel.AI.OpenResponses/OpenResponsesCustomToolFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L36)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Name_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


The tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L30)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_NeedsApproval_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_NeedsApproval"></a>

### NeedsApproval


```csharp
public bool NeedsApproval { get; set; }
```


When true, a call needs approval before it runs. Maps to `needsApproval: true`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L42)

<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_NeedsApprovalFunction_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_NeedsApprovalFunction"></a>

### NeedsApprovalFunction


```csharp
public Func<string, ToolNeedsApprovalOptions, Task<bool>>? NeedsApprovalFunction { get; set; }
```


Decides for each call whether it needs approval. Used instead of [NeedsApproval](#Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_NeedsApproval).

**Value:** [Func&lt;string, ToolNeedsApprovalOptions, Task&lt;bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L45)
