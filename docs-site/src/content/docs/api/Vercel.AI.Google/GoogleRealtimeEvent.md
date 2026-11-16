---
title: "GoogleRealtimeEvent"
description: "One normalized Gemini Live event."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeEvent"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeEvent"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeEvent
```


One normalized Gemini Live event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L12)

## Constructors

<a id="Vercel_AI_Google_GoogleRealtimeEvent__ctor_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent__ctor_System_String_"></a>

### GoogleRealtimeEvent(string)


```csharp
public GoogleRealtimeEvent(string type)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L15)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Arguments_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Arguments"></a>

### Arguments


```csharp
public string? Arguments { get; set; }
```


Completed tool arguments.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L39)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_CallId_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_CallId"></a>

### CallId


```csharp
public string? CallId { get; set; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L30)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Delta_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Delta"></a>

### Delta


```csharp
public string? Delta { get; set; }
```


Text, audio, or argument payload.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L36)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_ItemId_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_ItemId"></a>

### ItemId


```csharp
public string? ItemId { get; set; }
```


Item id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L27)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Name_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Name"></a>

### Name


```csharp
public string? Name { get; set; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L33)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Raw_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; set; }
```


Provider message this event was parsed from.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L51)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_RawType_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_RawType"></a>

### RawType


```csharp
public string? RawType { get; set; }
```


Custom raw type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L45)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_ResponseId_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; set; }
```


Response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L24)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Status_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Status"></a>

### Status


```csharp
public string? Status { get; set; }
```


Response status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L48)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Transcript_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Transcript"></a>

### Transcript


```csharp
public string? Transcript { get; set; }
```


Transcript text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L42)

<a id="Vercel_AI_Google_GoogleRealtimeEvent_Type_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEvent_Type"></a>

### Type


```csharp
public string Type { get; }
```


Event type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L21)
