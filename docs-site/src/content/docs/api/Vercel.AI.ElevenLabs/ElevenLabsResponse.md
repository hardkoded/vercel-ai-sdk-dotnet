---
title: "ElevenLabsResponse"
description: "Response metadata for one ElevenLabs call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsResponse"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsResponse
```


Response metadata for one ElevenLabs call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L116)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__"></a>

### `ElevenLabsResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>, JsonElement?)`


```csharp
public ElevenLabsResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, JsonElement? body)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L119)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_Body_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


JSON response body. Speech calls return audio, so this is null for them.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L137)

<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_Headers_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L134)

<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_ModelId_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L131)

<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_Timestamp_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L128)
