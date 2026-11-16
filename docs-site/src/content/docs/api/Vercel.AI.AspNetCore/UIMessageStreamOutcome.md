---
title: "UIMessageStreamOutcome"
description: "The operation-level outcome of a UI message stream. This is separate from model finish reasons and individual stream chunks. Fatal stream-processing failures..."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamOutcome"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamOutcome
```


The operation-level outcome of a UI message stream. This is separate from model finish reasons and individual stream chunks.
Fatal stream-processing failures override outcomes declared by the stream owner.
Consumer cancellation before an outcome is declared keeps the [Unknown](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcomeStatus/#Vercel_AI_AspNetCore_UIMessageStreamOutcomeStatus_Unknown) status and is reported
separately through [IsCancelled](/api/Vercel.AI.AspNetCore/UIMessageStreamOnEndEvent/#Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsCancelled). Maps to `UIMessageStreamOutcome`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L31)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome__ctor_Vercel_AI_AspNetCore_UIMessageStreamOutcomeStatus_System_Exception_"></a>

### UIMessageStreamOutcome(UIMessageStreamOutcomeStatus, Exception?)


```csharp
public UIMessageStreamOutcome(UIMessageStreamOutcomeStatus status, Exception? error = null)
```


Creates an outcome.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [UIMessageStreamOutcomeStatus](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcomeStatus/) |  |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L34)

## Properties

<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome_Error_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome_Error"></a>

### Error


```csharp
public Exception? Error { get; }
```


The error of a [Failed](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcomeStatus/#Vercel_AI_AspNetCore_UIMessageStreamOutcomeStatus_Failed) outcome, when there is one.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L44)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome_Status_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOutcome_Status"></a>

### Status


```csharp
public UIMessageStreamOutcomeStatus Status { get; }
```


The status.

**Value:** [UIMessageStreamOutcomeStatus](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcomeStatus/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L41)
