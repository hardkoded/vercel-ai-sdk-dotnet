---
title: "GoogleRealtime"
description: "Maps Gemini Live WebSocket messages. No socket is opened, so this compiles on netstandard2.0."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtime"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtime"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtime
```


Maps Gemini Live WebSocket messages. No socket is opened, so this compiles on netstandard2.0.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L109)

## Methods

<a id="Vercel_AI_Google_GoogleRealtime_BuildSession_"></a>
<a id="Vercel_AI_Google_GoogleRealtime_BuildSession_Vercel_AI_Google_GoogleRealtimeSession_"></a>

### BuildSession(GoogleRealtimeSession)


```csharp
public static JsonObject BuildSession(GoogleRealtimeSession session)
```


Builds the Live `setup` object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `session` | [GoogleRealtimeSession](/api/Vercel.AI.Google/GoogleRealtimeSession/) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L227)

<a id="Vercel_AI_Google_GoogleRealtime_FunctionResponse_"></a>
<a id="Vercel_AI_Google_GoogleRealtime_FunctionResponse_System_String_"></a>

### FunctionResponse(string?)


```csharp
public static JsonNode FunctionResponse(string? output)
```


Wraps a non-object tool result in `{ output }` so the Live API receives a Struct.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `output` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L338)

<a id="Vercel_AI_Google_GoogleRealtime_ParseServerEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtime_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public IReadOnlyList<GoogleRealtimeEvent> ParseServerEvent(JsonElement raw)
```


Maps one server message into one or more normalized events.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;GoogleRealtimeEvent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L121)

<a id="Vercel_AI_Google_GoogleRealtime_SerializeClient_"></a>
<a id="Vercel_AI_Google_GoogleRealtime_SerializeClient_System_String_Vercel_AI_Google_GoogleRealtimeSession_System_String_System_String_System_String_System_String_System_String_"></a>

### SerializeClient(string, GoogleRealtimeSession?, string?, string?, string?, string?, string?)


```csharp
public JsonObject? SerializeClient(string type, GoogleRealtimeSession? session, string? audio, string? text, string? callId, string? callName, string? callOutput)
```


Serializes a client event. Returns null for events Gemini Live ignores.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `session` | [GoogleRealtimeSession](/api/Vercel.AI.Google/GoogleRealtimeSession/) |  |
| `audio` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `callName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `callOutput` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L193)
