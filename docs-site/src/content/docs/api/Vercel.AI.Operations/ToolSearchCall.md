---
title: "ToolSearchCall"
description: "A planned model tool call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchCall"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolSearchCall
```


A planned model tool call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L324)

## Constructors

<a id="Vercel_AI_Operations_ToolSearchCall__ctor_"></a>
<a id="Vercel_AI_Operations_ToolSearchCall__ctor_System_String_System_String_System_String_"></a>

### ToolSearchCall(string, string, string)


```csharp
public ToolSearchCall(string toolCallId, string toolName, string argumentsJson)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L327)

## Properties

<a id="Vercel_AI_Operations_ToolSearchCall_ArgumentsJson_"></a>
<a id="Vercel_AI_Operations_ToolSearchCall_ArgumentsJson"></a>

### ArgumentsJson


```csharp
public string ArgumentsJson { get; }
```


JSON arguments.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L341)

<a id="Vercel_AI_Operations_ToolSearchCall_ToolCallId_"></a>
<a id="Vercel_AI_Operations_ToolSearchCall_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L335)

<a id="Vercel_AI_Operations_ToolSearchCall_ToolName_"></a>
<a id="Vercel_AI_Operations_ToolSearchCall_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L338)
