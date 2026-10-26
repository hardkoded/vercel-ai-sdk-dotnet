---
title: "ToolSearchRun"
description: "The outcome of a tool-search generation."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchRun"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchRun"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolSearchRun
```


The outcome of a tool-search generation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L345)

## Properties

<a id="Vercel_AI_Operations_ToolSearchRun_Bindings_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_Bindings"></a>

### Bindings


```csharp
public List<List<string>> Bindings { get; }
```


Code-mode binding keys per step.

**Value:** [List&lt;List&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L360)

<a id="Vercel_AI_Operations_ToolSearchRun_ExecuteCount_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_ExecuteCount"></a>

### ExecuteCount


```csharp
public int ExecuteCount { get; set; }
```


How many times the discovered tool ran.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L363)

<a id="Vercel_AI_Operations_ToolSearchRun_ExecutedArguments_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_ExecutedArguments"></a>

### ExecutedArguments


```csharp
public string? ExecutedArguments { get; set; }
```


Arguments from the discovered tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L366)

<a id="Vercel_AI_Operations_ToolSearchRun_Steps_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_Steps"></a>

### Steps


```csharp
public List<ToolSearchStepRecord> Steps { get; }
```


Steps.

**Value:** [List&lt;ToolSearchStepRecord&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L357)

<a id="Vercel_AI_Operations_ToolSearchRun_Text_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_Text"></a>

### Text


```csharp
public string Text { get; set; }
```


Final text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L348)

<a id="Vercel_AI_Operations_ToolSearchRun_ToolNames_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_ToolNames"></a>

### ToolNames


```csharp
public List<List<string>> ToolNames { get; }
```


Tool names per model call.

**Value:** [List&lt;List&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L351)

<a id="Vercel_AI_Operations_ToolSearchRun_UserMessages_"></a>
<a id="Vercel_AI_Operations_ToolSearchRun_UserMessages"></a>

### UserMessages


```csharp
public List<List<string>> UserMessages { get; }
```


User messages per model call.

**Value:** [List&lt;List&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L354)
