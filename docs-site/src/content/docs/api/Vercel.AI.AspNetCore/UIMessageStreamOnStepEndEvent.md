---
title: "UIMessageStreamOnStepEndEvent"
description: "The event that OnStepEnd receives. Maps to UIMessageStreamOnStepEndCallback's event."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamOnStepEndEvent"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamOnStepEndEvent
```


The event that [OnStepEnd](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepEnd) receives. Maps to `UIMessageStreamOnStepEndCallback`'s event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L92)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Ui_UIMessage__System_Boolean_Vercel_AI_Ui_UIMessage_"></a>

### `UIMessageStreamOnStepEndEvent(IReadOnlyList<UIMessage>, bool, UIMessage)`


```csharp
public UIMessageStreamOnStepEndEvent(IReadOnlyList<UIMessage> messages, bool isContinuation, UIMessage responseMessage)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `isContinuation` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `responseMessage` | [UIMessage](/api/Vercel.AI.Ui/UIMessage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L95)

## Properties

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent_IsContinuation_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent_IsContinuation"></a>

### IsContinuation


```csharp
public bool IsContinuation { get; }
```


Whether the response message continues the last original message, or a new message was created.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L110)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent_Messages_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<UIMessage> Messages { get; }
```


The updated list of UI messages at the end of this step.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L107)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent_ResponseMessage_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOnStepEndEvent_ResponseMessage"></a>

### ResponseMessage


```csharp
public UIMessage ResponseMessage { get; }
```


The message that was sent to the client as a response, including the original message when it was extended.

**Value:** [UIMessage](/api/Vercel.AI.Ui/UIMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStreamOutcome.cs#L113)
