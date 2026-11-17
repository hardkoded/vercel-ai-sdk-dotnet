---
title: "RealtimeModelV4ClientEvent"
description: "A normalized event sent by the client. Maps to the RealtimeModelV4ClientEvent union: Type is the tag (session-update, session-start, session-close, input-aud..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4ClientEvent"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4ClientEvent
```


A normalized event sent by the client. Maps to the `RealtimeModelV4ClientEvent` union: [Type](#Vercel_AI_Provider_RealtimeModelV4ClientEvent_Type) is
the tag (`session-update`, `session-start`, `session-close`, `input-audio-mute`,
`input-audio-unmute`, `context-append`, `input-audio-append`, `input-audio-commit`,
`input-audio-clear`, `conversation-item-create`, `conversation-item-truncate`,
`response-create`, `response-cancel`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L258)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent__ctor_System_String_"></a>

### RealtimeModelV4ClientEvent(string)


```csharp
public RealtimeModelV4ClientEvent(string type)
```


Creates an event with a union tag.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L261)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Audio_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Audio"></a>

### Audio


```csharp
public string? Audio { get; set; }
```


Base64 audio for `input-audio-append`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L282)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_AudioEndMs_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_AudioEndMs"></a>

### AudioEndMs


```csharp
public int? AudioEndMs { get; set; }
```


Audio end in milliseconds for `conversation-item-truncate`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L294)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Config_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Config"></a>

### Config


```csharp
public RealtimeModelV4SessionConfig? Config { get; set; }
```


Config for `session-update` and `session-start`.

**Value:** [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L273)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Content_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Content"></a>

### Content


```csharp
public string? Content { get; set; }
```


Content for `context-append`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L276)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_ContentIndex_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_ContentIndex"></a>

### ContentIndex


```csharp
public int? ContentIndex { get; set; }
```


Content index for `conversation-item-truncate`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L291)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_DelegationId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_DelegationId"></a>

### DelegationId


```csharp
public string? DelegationId { get; set; }
```


Delegation id for `context-append`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L279)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_EventId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_EventId"></a>

### EventId


```csharp
public string? EventId { get; set; }
```


Event id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L270)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Item_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Item"></a>

### Item


```csharp
public RealtimeModelV4ConversationItem? Item { get; set; }
```


Item for `conversation-item-create`.

**Value:** [RealtimeModelV4ConversationItem](/api/Vercel.AI.Provider/RealtimeModelV4ConversationItem/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L285)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_ItemId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_ItemId"></a>

### ItemId


```csharp
public string? ItemId { get; set; }
```


Item id for `conversation-item-truncate`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L288)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Options_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Options"></a>

### Options


```csharp
public RealtimeModelV4ResponseOptions? Options { get; set; }
```


Options for `response-create`.

**Value:** [RealtimeModelV4ResponseOptions](/api/Vercel.AI.Provider/RealtimeModelV4ResponseOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L297)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonObject? ProviderOptions { get; set; }
```


Provider options for `context-append`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L300)

<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Type_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ClientEvent_Type"></a>

### Type


```csharp
public string Type { get; }
```


The union tag.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L267)
