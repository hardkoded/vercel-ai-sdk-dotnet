---
title: "ToolCallPart"
description: "A tool call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallPart"
  hidden: true
---

<a id="Vercel_AI_ToolCallPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolCallPart : TextStreamPart
```


A tool call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1175)

## Constructors

<a id="Vercel_AI_ToolCallPart__ctor_"></a>
<a id="Vercel_AI_ToolCallPart__ctor_Vercel_AI_Provider_GeneratedToolCall_"></a>

### ToolCallPart(GeneratedToolCall)


```csharp
public ToolCallPart(GeneratedToolCall call)
```


Creates a tool-call part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1178)

## Properties

<a id="Vercel_AI_ToolCallPart_ToolCall_"></a>
<a id="Vercel_AI_ToolCallPart_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


Tool call.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1185)
