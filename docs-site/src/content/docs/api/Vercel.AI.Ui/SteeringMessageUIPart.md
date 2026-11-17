---
title: "SteeringMessageUIPart"
description: "A user message that steered the run, shown at the step that received it. Maps to SteeringMessageUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "SteeringMessageUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_SteeringMessageUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class SteeringMessageUIPart : UIMessagePart
```


A user message that steered the run, shown at the step that received it. Maps to `SteeringMessageUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L398)

## Constructors

<a id="Vercel_AI_Ui_SteeringMessageUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_SteeringMessageUIPart__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Ui_UIMessagePart__"></a>

### `SteeringMessageUIPart(string, IReadOnlyList<UIMessagePart>)`


```csharp
public SteeringMessageUIPart(string id, IReadOnlyList<UIMessagePart> parts)
```


Creates a steering message part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `parts` | [IReadOnlyList&lt;UIMessagePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L401)

## Properties

<a id="Vercel_AI_Ui_SteeringMessageUIPart_Id_"></a>
<a id="Vercel_AI_Ui_SteeringMessageUIPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


The id of the steering message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L409)

<a id="Vercel_AI_Ui_SteeringMessageUIPart_Parts_"></a>
<a id="Vercel_AI_Ui_SteeringMessageUIPart_Parts"></a>

### Parts


```csharp
public IReadOnlyList<UIMessagePart> Parts { get; }
```


The content of the user message: text and file parts.

**Value:** [IReadOnlyList&lt;UIMessagePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L412)
