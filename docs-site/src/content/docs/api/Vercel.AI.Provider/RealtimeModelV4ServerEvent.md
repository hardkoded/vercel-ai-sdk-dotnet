---
title: "RealtimeModelV4ServerEvent"
description: "A normalized event emitted by a realtime model (model to client). Maps to the RealtimeModelV4ServerEvent union: Type is the union tag (for example response-d..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4ServerEvent"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4ServerEvent
```


A normalized event emitted by a realtime model (model to client). Maps to the `RealtimeModelV4ServerEvent`
union: [Type](#Vercel_AI_Provider_RealtimeModelV4ServerEvent_Type) is the union tag (for example `response-done`) and the other members are the
fields that tag carries; fields a tag does not define stay null. [Raw](#Vercel_AI_Provider_RealtimeModelV4ServerEvent_Raw) holds the original provider event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L15)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent__ctor_System_String_"></a>

### RealtimeModelV4ServerEvent(string)


```csharp
public RealtimeModelV4ServerEvent(string type)
```


Creates an event with a union tag.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L18)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Arguments_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Arguments"></a>

### Arguments


```csharp
public string? Arguments { get; set; }
```


Complete JSON arguments for `function-call-arguments-done`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L48)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_CallId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_CallId"></a>

### CallId


```csharp
public string? CallId { get; set; }
```


Function call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L39)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ClientEventId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ClientEventId"></a>

### ClientEventId


```csharp
public string? ClientEventId { get; set; }
```


Client event id for `error` and `command-acknowledged`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L66)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Code_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Code"></a>

### Code


```csharp
public string? Code { get; set; }
```


Error code for `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L63)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Command_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Command"></a>

### Command


```csharp
public string? Command { get; set; }
```


Acknowledged command name for `command-acknowledged`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L102)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ContextWindowUsageRatio_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ContextWindowUsageRatio"></a>

### ContextWindowUsageRatio


```csharp
public double? ContextWindowUsageRatio { get; set; }
```


Context window usage ratio (0 to 1) for `session-usage`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L81)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_DelegationId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_DelegationId"></a>

### DelegationId


```csharp
public string? DelegationId { get; set; }
```


Delegation id for `delegation-created`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L93)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_DelegationMode_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_DelegationMode"></a>

### DelegationMode


```csharp
public string? DelegationMode { get; set; }
```


Delegation mode for `session-started`: `client` or `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L78)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Delta_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Delta"></a>

### Delta


```csharp
public string? Delta { get; set; }
```


Delta text or base64 audio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L45)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_EndMs_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_EndMs"></a>

### EndMs


```csharp
public double? EndMs { get; set; }
```


Fragment end in milliseconds for `transcript-fragment`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L90)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Item_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Item"></a>

### Item


```csharp
public JsonElement? Item { get; set; }
```


The conversation item for `conversation-item-added`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L105)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ItemId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ItemId"></a>

### ItemId


```csharp
public string? ItemId { get; set; }
```


Item id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L33)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Message_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Message"></a>

### Message


```csharp
public string? Message { get; set; }
```


Error message for `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L60)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Name_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Name"></a>

### Name


```csharp
public string? Name { get; set; }
```


Function name for `function-call-arguments-done`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L42)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_OffsetMs_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_OffsetMs"></a>

### OffsetMs


```csharp
public double? OffsetMs { get; set; }
```


Offset in milliseconds for `delegation-created`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L99)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_PreviousItemId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_PreviousItemId"></a>

### PreviousItemId


```csharp
public string? PreviousItemId { get; set; }
```


Previous item id for `audio-committed`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L36)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Raw_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; set; }
```


The original provider event.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L108)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_RawType_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_RawType"></a>

### RawType


```csharp
public string? RawType { get; set; }
```


The original provider event type for `custom` events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L69)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Reason_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Reason"></a>

### Reason


```csharp
public string? Reason { get; set; }
```


Close reason for `session-closed`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L72)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ResponseId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; set; }
```


Response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L30)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_SessionId_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_SessionId"></a>

### SessionId


```csharp
public string? SessionId { get; set; }
```


Session id for `session-started`, `session-closed` and `session-created`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L27)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Speaker_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Speaker"></a>

### Speaker


```csharp
public string? Speaker { get; set; }
```


Speaker for `transcript-fragment`: `user` or `assistant`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L84)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_StartMs_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_StartMs"></a>

### StartMs


```csharp
public double? StartMs { get; set; }
```


Fragment start in milliseconds for `transcript-fragment`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L87)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Status_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Status"></a>

### Status


```csharp
public string? Status { get; set; }
```


Response status for `response-done`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L57)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Target_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Target"></a>

### Target


```csharp
public string? Target { get; set; }
```


Delegation target for `delegation-created`: `client` or `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L96)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Text_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text for `text-done`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L54)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Transcript_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Transcript"></a>

### Transcript


```csharp
public string? Transcript { get; set; }
```


Transcript for transcription and `audio-transcript-done` events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L51)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Type_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Type"></a>

### Type


```csharp
public string Type { get; }
```


The union tag.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L24)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Usage_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerEvent_Usage"></a>

### Usage


```csharp
public RealtimeModelV4Usage? Usage { get; set; }
```


Normalized usage on completion events or provider snapshots.

**Value:** [RealtimeModelV4Usage](/api/Vercel.AI.Provider/RealtimeModelV4Usage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L75)
