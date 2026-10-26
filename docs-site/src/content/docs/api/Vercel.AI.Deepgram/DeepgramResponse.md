---
title: "DeepgramResponse"
description: "Response metadata for one Deepgram call."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramResponse"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramResponse"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramResponse
```


Response metadata for one Deepgram call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L97)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramResponse__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__"></a>

### `DeepgramResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>, JsonElement?)`


```csharp
public DeepgramResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, JsonElement? body)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L100)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramResponse_Body_"></a>
<a id="Vercel_AI_Deepgram_DeepgramResponse_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


JSON response body. Speech calls return audio, so this is null for them.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L118)

<a id="Vercel_AI_Deepgram_DeepgramResponse_Headers_"></a>
<a id="Vercel_AI_Deepgram_DeepgramResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L115)

<a id="Vercel_AI_Deepgram_DeepgramResponse_ModelId_"></a>
<a id="Vercel_AI_Deepgram_DeepgramResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L112)

<a id="Vercel_AI_Deepgram_DeepgramResponse_Timestamp_"></a>
<a id="Vercel_AI_Deepgram_DeepgramResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L109)
