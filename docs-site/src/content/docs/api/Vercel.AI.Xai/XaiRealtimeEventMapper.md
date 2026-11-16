---
title: "XaiRealtimeEventMapper"
description: "Maps xAI realtime wire events to normalized events and back. Maps to the exported functions of xai-realtime-event-mapper.ts: parseXaiRealtimeServerEvent, ser..."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiRealtimeEventMapper"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiRealtimeEventMapper"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public static class XaiRealtimeEventMapper
```


Maps xAI realtime wire events to normalized events and back. Maps to the exported functions of
`xai-realtime-event-mapper.ts`: `parseXaiRealtimeServerEvent`, `serializeXaiRealtimeClientEvent`
and `buildXaiSessionConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeEventMapper.cs#L16)

## Methods

<a id="Vercel_AI_Xai_XaiRealtimeEventMapper_BuildXaiSessionConfig_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeEventMapper_BuildXaiSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_"></a>

### BuildXaiSessionConfig(RealtimeModelV4SessionConfig)


```csharp
public static JsonObject BuildXaiSessionConfig(RealtimeModelV4SessionConfig config)
```


Builds the xAI `session` object from a normalized session config. Maps to `buildXaiSessionConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeEventMapper.cs#L227)

<a id="Vercel_AI_Xai_XaiRealtimeEventMapper_ParseXaiRealtimeServerEvent_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeEventMapper_ParseXaiRealtimeServerEvent_System_Text_Json_JsonElement_"></a>

### ParseXaiRealtimeServerEvent(JsonElement)


```csharp
public static RealtimeModelV4ServerEvent ParseXaiRealtimeServerEvent(JsonElement raw)
```


Maps one xAI server event. Maps to `parseXaiRealtimeServerEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [RealtimeModelV4ServerEvent](/api/Vercel.AI.Provider/RealtimeModelV4ServerEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeEventMapper.cs#L19)

<a id="Vercel_AI_Xai_XaiRealtimeEventMapper_SerializeXaiRealtimeClientEvent_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeEventMapper_SerializeXaiRealtimeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_"></a>

### SerializeXaiRealtimeClientEvent(RealtimeModelV4ClientEvent)


```csharp
public static JsonObject? SerializeXaiRealtimeClientEvent(RealtimeModelV4ClientEvent clientEvent)
```


Maps one normalized client event to the xAI wire event. Returns null when xAI has no equivalent. Maps to `serializeXaiRealtimeClientEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeEventMapper.cs#L148)
