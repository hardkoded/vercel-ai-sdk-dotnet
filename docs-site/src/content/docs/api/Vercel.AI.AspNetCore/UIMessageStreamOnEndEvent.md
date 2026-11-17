---
title: "UIMessageStreamOnEndEvent"
description: "The event that OnEnd receives. Maps to UIMessageStreamOnEndCallback's event."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamOnEndEvent"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamOnEndEvent
```


The event that [OnEnd](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnEnd) receives. Maps to `UIMessageStreamOnEndCallback`'s event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L48)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Ui_UIMessage__System_Boolean_System_Boolean_Vercel_AI_AspNetCore_UIMessageStreamOutcome_Vercel_AI_Ui_UIMessage_"></a>

### `UIMessageStreamOnEndEvent(IReadOnlyList<UIMessage>, bool, bool, UIMessageStreamOutcome, UIMessage)`


```csharp
public UIMessageStreamOnEndEvent(IReadOnlyList<UIMessage> messages, bool isContinuation, bool isAborted, UIMessageStreamOutcome outcome, UIMessage responseMessage)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `isContinuation` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `isAborted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `outcome` | [UIMessageStreamOutcome](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcome/) |  |
| `responseMessage` | [UIMessage](/api/Vercel.AI.Ui/UIMessage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L51)

## Properties

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_FinishReason_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_FinishReason"></a>

### FinishReason


```csharp
public string? FinishReason { get; init; }
```


The reason why the generation finished, as the `finish` chunk wrote it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L88)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsAborted_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsAborted"></a>

### IsAborted


```csharp
public bool IsAborted { get; }
```


Whether the stream was aborted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L73)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsCancelled_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsCancelled"></a>

### IsCancelled


```csharp
public bool IsCancelled { get; init; }
```


True when the consumer cancelled the stream before an outcome was declared, for example because the client disconnected.
Maps to `isCancelled`, which is only present when it is `true`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L79)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsContinuation_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_IsContinuation"></a>

### IsContinuation


```csharp
public bool IsContinuation { get; }
```


Whether the response message continues the last original message, or a new message was created.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L70)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_Messages_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<UIMessage> Messages { get; }
```


The updated list of UI messages.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L67)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_Outcome_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_Outcome"></a>

### Outcome


```csharp
public UIMessageStreamOutcome Outcome { get; }
```


The operation-level outcome of the stream.

**Value:** [UIMessageStreamOutcome](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcome/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L82)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_ResponseMessage_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnEndEvent_ResponseMessage"></a>

### ResponseMessage


```csharp
public UIMessage ResponseMessage { get; }
```


The message that was sent to the client as a response, including the original message when it was extended.

**Value:** [UIMessage](/api/Vercel.AI.Ui/UIMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L85)
