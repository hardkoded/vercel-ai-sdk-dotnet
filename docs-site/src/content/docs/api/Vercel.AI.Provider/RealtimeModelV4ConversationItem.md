---
title: "RealtimeModelV4ConversationItem"
description: "A conversation item created by the client. Maps to RealtimeModelV4ConversationItem; Type is text-message, audio-message or function-call-output."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4ConversationItem"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4ConversationItem
```


A conversation item created by the client. Maps to `RealtimeModelV4ConversationItem`; [Type](#Vercel_AI_Provider_RealtimeModelV4ConversationItem_Type) is
`text-message`, `audio-message` or `function-call-output`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L208)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem__ctor_System_String_"></a>

### RealtimeModelV4ConversationItem(string)


```csharp
public RealtimeModelV4ConversationItem(string type)
```


Creates an item.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L211)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Audio_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Audio"></a>

### Audio


```csharp
public string? Audio { get; set; }
```


Base64 audio of an `audio-message`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L226)

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_CallId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_CallId"></a>

### CallId


```csharp
public string? CallId { get; set; }
```


Call id of a `function-call-output`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L229)

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Name_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Name"></a>

### Name


```csharp
public string? Name { get; set; }
```


Function name of a `function-call-output`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L232)

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Output_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Output"></a>

### Output


```csharp
public string? Output { get; set; }
```


Output of a `function-call-output`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L235)

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Role_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Role"></a>

### Role


```csharp
public string? Role { get; set; }
```


Role (`user`) for message items.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L220)

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Text_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text of a `text-message`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L223)

<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Type_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ConversationItem_Type"></a>

### Type


```csharp
public string Type { get; }
```


Union tag.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L217)
