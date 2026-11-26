---
title: "ToolOutputDeniedPart"
description: "A tool call was denied, so its tool did not run. Maps to the tool-output-denied stream part."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolOutputDeniedPart"
  hidden: true
---

<a id="Vercel_AI_ToolOutputDeniedPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolOutputDeniedPart : TextStreamPart
```


A tool call was denied, so its tool did not run. Maps to the `tool-output-denied` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2903)

## Constructors

<a id="Vercel_AI_ToolOutputDeniedPart__ctor_"></a>
<a id="Vercel_AI_ToolOutputDeniedPart__ctor_System_String_System_String_"></a>

### ToolOutputDeniedPart(string, string)


```csharp
public ToolOutputDeniedPart(string toolCallId, string toolName)
```


Creates a tool-output-denied part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2906)

## Properties

<a id="Vercel_AI_ToolOutputDeniedPart_ToolCallId_"></a>
<a id="Vercel_AI_ToolOutputDeniedPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the denied tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2914)

<a id="Vercel_AI_ToolOutputDeniedPart_ToolName_"></a>
<a id="Vercel_AI_ToolOutputDeniedPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Name of the denied tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2917)
