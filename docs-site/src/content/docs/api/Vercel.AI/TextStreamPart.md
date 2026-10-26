---
title: "TextStreamPart"
description: "A part of the streamText full stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextStreamPart"
  hidden: true
---

<a id="Vercel_AI_TextStreamPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public abstract class TextStreamPart
```


A part of the `streamText` full stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AbortPart](/api/Vercel.AI/AbortPart/), [ErrorPart](/api/Vercel.AI/ErrorPart/), [FinishPart](/api/Vercel.AI/FinishPart/), [RawPart](/api/Vercel.AI/RawPart/), [ReasoningDeltaPart](/api/Vercel.AI/ReasoningDeltaPart/), [SourcePart](/api/Vercel.AI/SourcePart/), [StartPart](/api/Vercel.AI/StartPart/), [StartStepPart](/api/Vercel.AI/StartStepPart/), [StepFinishPart](/api/Vercel.AI/StepFinishPart/), [TextDeltaPart](/api/Vercel.AI/TextDeltaPart/), [ToolApprovalRequestPart](/api/Vercel.AI/ToolApprovalRequestPart/), [ToolCallPart](/api/Vercel.AI/ToolCallPart/), [ToolErrorPart](/api/Vercel.AI/ToolErrorPart/), [ToolInputDeltaPart](/api/Vercel.AI/ToolInputDeltaPart/), [ToolInputEndPart](/api/Vercel.AI/ToolInputEndPart/), [ToolInputStartPart](/api/Vercel.AI/ToolInputStartPart/), [ToolResultPart](/api/Vercel.AI/ToolResultPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1000)

## Constructors

<a id="Vercel_AI_TextStreamPart__ctor_"></a>
<a id="Vercel_AI_TextStreamPart__ctor_System_String_"></a>

### TextStreamPart(string)


```csharp
protected TextStreamPart(string type)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1003)

## Properties

<a id="Vercel_AI_TextStreamPart_Type_"></a>
<a id="Vercel_AI_TextStreamPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1009)
