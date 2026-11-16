---
title: "GoogleRealtimeEventMapper"
description: "Stateful event mapper for Google's Gemini Live API. Google's events have no response or item ids and a single message can carry several pieces of data, so th..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeEventMapper"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeEventMapper"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeEventMapper
```


Stateful event mapper for Google's Gemini Live API. Google's events have no response or item ids and a single
message can carry several pieces of data, so this class tracks turn state to generate consistent synthetic ids.
No socket is opened, so this compiles on netstandard2.0. Maps to `GoogleRealtimeEventMapper` and
`buildGoogleSessionConfig` in `google-realtime-event-mapper.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeEventMapper.cs#L19)

## Methods

<a id="Vercel_AI_Google_GoogleRealtimeEventMapper_BuildGoogleSessionConfig_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEventMapper_BuildGoogleSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_String_"></a>

### BuildGoogleSessionConfig(RealtimeModelV4SessionConfig?, string)


```csharp
public static JsonObject BuildGoogleSessionConfig(RealtimeModelV4SessionConfig? config, string modelId)
```


Builds a Google-specific session configuration from a normalized config. Used to construct the
`bidiGenerateContentSetup` payload for auth token creation. Maps to `buildGoogleSessionConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeEventMapper.cs#L55)

<a id="Vercel_AI_Google_GoogleRealtimeEventMapper_ParseServerEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEventMapper_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
```


Maps one Gemini Live server message into normalized events. Only a shared wire frame associates
`usageMetadata` with a `response-done`; any other usage snapshot becomes a `custom` event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;RealtimeModelV4ServerEvent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeEventMapper.cs#L188)

<a id="Vercel_AI_Google_GoogleRealtimeEventMapper_SerializeClientEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeEventMapper_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_String_"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, string)


```csharp
public JsonObject? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, string modelId)
```


Serializes a normalized client event. Returns null for events Gemini Live ignores.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeEventMapper.cs#L222)
