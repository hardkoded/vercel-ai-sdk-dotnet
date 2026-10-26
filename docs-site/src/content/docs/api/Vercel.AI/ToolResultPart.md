---
title: "ToolResultPart"
description: "A tool result."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolResultPart"
  hidden: true
---

<a id="Vercel_AI_ToolResultPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolResultPart : TextStreamPart
```


A tool result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1288)

## Constructors

<a id="Vercel_AI_ToolResultPart__ctor_"></a>
<a id="Vercel_AI_ToolResultPart__ctor_Vercel_AI_ExecutedTool_"></a>

### ToolResultPart(ExecutedTool)


```csharp
public ToolResultPart(ExecutedTool result)
```


Creates a tool-result part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [ExecutedTool](/api/Vercel.AI/ExecutedTool/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1291)

## Properties

<a id="Vercel_AI_ToolResultPart_Result_"></a>
<a id="Vercel_AI_ToolResultPart_Result"></a>

### Result


```csharp
public ExecutedTool Result { get; }
```


Tool result.

**Value:** [ExecutedTool](/api/Vercel.AI/ExecutedTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1298)
