---
title: "AbortPart"
description: "The call was aborted. It is the last part of the stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "AbortPart"
  hidden: true
---

<a id="Vercel_AI_AbortPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class AbortPart : TextStreamPart
```


The call was aborted. It is the last part of the stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1439)

## Constructors

<a id="Vercel_AI_AbortPart__ctor_"></a>
<a id="Vercel_AI_AbortPart__ctor_System_String_"></a>

### AbortPart(string?)


```csharp
public AbortPart(string? reason)
```


Creates an abort part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1442)

## Properties

<a id="Vercel_AI_AbortPart_Reason_"></a>
<a id="Vercel_AI_AbortPart_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Abort reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1449)
