---
title: "ToolSearchStepRecord"
description: "One model step produced by ToolSearchGeneration."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchStepRecord"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchStepRecord"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolSearchStepRecord
```


One model step produced by [ToolSearchGeneration](/api/Vercel.AI.Operations/ToolSearchGeneration/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L286)

## Properties

<a id="Vercel_AI_Operations_ToolSearchStepRecord_Content_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepRecord_Content"></a>

### Content


```csharp
public List<ToolSearchStepItem> Content { get; }
```


Tool results and tool errors.

**Value:** [List&lt;ToolSearchStepItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L295)

<a id="Vercel_AI_Operations_ToolSearchStepRecord_ToolNames_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepRecord_ToolNames"></a>

### ToolNames


```csharp
public List<string> ToolNames { get; }
```


Tool names sent to the model.

**Value:** [List&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L289)

<a id="Vercel_AI_Operations_ToolSearchStepRecord_UserMessages_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepRecord_UserMessages"></a>

### UserMessages


```csharp
public List<string> UserMessages { get; }
```


User messages, including a catalog message when code mode publishes one.

**Value:** [List&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L292)
