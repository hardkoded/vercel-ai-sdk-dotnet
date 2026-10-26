---
title: "ToolSearchStepItem"
description: "A tool result or tool error from one step."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchStepItem"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchStepItem"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolSearchStepItem
```


A tool result or tool error from one step.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L299)

## Constructors

<a id="Vercel_AI_Operations_ToolSearchStepItem__ctor_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepItem__ctor_System_String_System_String_System_String_System_Object_"></a>

### ToolSearchStepItem(string, string, string?, object?)


```csharp
public ToolSearchStepItem(string type, string toolCallId, string? toolName, object? output)
```


Creates an item.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `output` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L302)

## Properties

<a id="Vercel_AI_Operations_ToolSearchStepItem_Output_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepItem_Output"></a>

### Output


```csharp
public object? Output { get; }
```


Result value.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L320)

<a id="Vercel_AI_Operations_ToolSearchStepItem_ToolCallId_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepItem_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L314)

<a id="Vercel_AI_Operations_ToolSearchStepItem_ToolName_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepItem_ToolName"></a>

### ToolName


```csharp
public string? ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L317)

<a id="Vercel_AI_Operations_ToolSearchStepItem_Type_"></a>
<a id="Vercel_AI_Operations_ToolSearchStepItem_Type"></a>

### Type


```csharp
public string Type { get; }
```


`tool-result` or `tool-error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L311)
