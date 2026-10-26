---
title: "ElevenLabsSpeechResult"
description: "Synthesized ElevenLabs audio."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsSpeechResult"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsSpeechResult
```


Synthesized ElevenLabs audio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L141)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult__ctor_System_Byte___System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__System_String_Vercel_AI_ElevenLabs_ElevenLabsResponse_"></a>

### `ElevenLabsSpeechResult(byte[], string, IReadOnlyList<ModelWarning>, string, ElevenLabsResponse)`


```csharp
public ElevenLabsSpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, ElevenLabsResponse response)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `requestBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `response` | [ElevenLabsResponse](/api/Vercel.AI.ElevenLabs/ElevenLabsResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L144)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_Audio_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L154)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_MediaType_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Response media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L157)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_RequestBody_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_RequestBody"></a>

### RequestBody


```csharp
public string RequestBody { get; }
```


JSON request body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L163)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_Response_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_Response"></a>

### Response


```csharp
public ElevenLabsResponse Response { get; }
```


Response metadata.

**Value:** [ElevenLabsResponse](/api/Vercel.AI.ElevenLabs/ElevenLabsResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L166)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_Warnings_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for ignored settings.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L160)
