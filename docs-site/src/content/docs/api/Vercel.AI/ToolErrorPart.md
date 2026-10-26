---
title: "ToolErrorPart"
description: "A tool call failed or was denied. A successful call is a ToolResultPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolErrorPart"
  hidden: true
---

<a id="Vercel_AI_ToolErrorPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolErrorPart : TextStreamPart
```


A tool call failed or was denied. A successful call is a [ToolResultPart](/api/Vercel.AI/ToolResultPart/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1302)

## Constructors

<a id="Vercel_AI_ToolErrorPart__ctor_"></a>
<a id="Vercel_AI_ToolErrorPart__ctor_Vercel_AI_ExecutedTool_"></a>

### ToolErrorPart(ExecutedTool)


```csharp
public ToolErrorPart(ExecutedTool result)
```


Creates a tool-error part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [ExecutedTool](/api/Vercel.AI/ExecutedTool/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1305)

## Properties

<a id="Vercel_AI_ToolErrorPart_Result_"></a>
<a id="Vercel_AI_ToolErrorPart_Result"></a>

### Result


```csharp
public ExecutedTool Result { get; }
```


The failed tool result. [OutputJson](/api/Vercel.AI/ExecutedTool/#Vercel_AI_ExecutedTool_OutputJson) holds the error.

**Value:** [ExecutedTool](/api/Vercel.AI/ExecutedTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1312)
