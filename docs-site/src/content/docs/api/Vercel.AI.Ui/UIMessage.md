---
title: "UIMessage"
description: "A message shown in a chat UI, made of parts. Maps to UIMessage."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessage"
  hidden: true
---

<a id="Vercel_AI_Ui_UIMessage"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class UIMessage
```


A message shown in a chat UI, made of parts. Maps to `UIMessage`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L26)

## Constructors

<a id="Vercel_AI_Ui_UIMessage__ctor_"></a>
<a id="Vercel_AI_Ui_UIMessage__ctor_Vercel_AI_Ui_UIMessageRole_System_Collections_Generic_IReadOnlyList_Vercel_AI_Ui_UIMessagePart__"></a>

### `UIMessage(UIMessageRole, IReadOnlyList<UIMessagePart>)`


```csharp
public UIMessage(UIMessageRole role, IReadOnlyList<UIMessagePart> parts)
```


Creates a UI message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `role` | [UIMessageRole](/api/Vercel.AI.Ui/UIMessageRole/) |  |
| `parts` | [IReadOnlyList&lt;UIMessagePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L29)

## Properties

<a id="Vercel_AI_Ui_UIMessage_Id_"></a>
<a id="Vercel_AI_Ui_UIMessage_Id"></a>

### Id


```csharp
public string Id { get; init; }
```


A unique identifier for the message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L36)

<a id="Vercel_AI_Ui_UIMessage_Metadata_"></a>
<a id="Vercel_AI_Ui_UIMessage_Metadata"></a>

### Metadata


```csharp
public JsonElement? Metadata { get; init; }
```


The metadata of the message.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L42)

<a id="Vercel_AI_Ui_UIMessage_Parts_"></a>
<a id="Vercel_AI_Ui_UIMessage_Parts"></a>

### Parts


```csharp
public IReadOnlyList<UIMessagePart> Parts { get; }
```


The parts of the message.

**Value:** [IReadOnlyList&lt;UIMessagePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L45)

<a id="Vercel_AI_Ui_UIMessage_Role_"></a>
<a id="Vercel_AI_Ui_UIMessage_Role"></a>

### Role


```csharp
public UIMessageRole Role { get; }
```


The role of the message.

**Value:** [UIMessageRole](/api/Vercel.AI.Ui/UIMessageRole/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L39)
