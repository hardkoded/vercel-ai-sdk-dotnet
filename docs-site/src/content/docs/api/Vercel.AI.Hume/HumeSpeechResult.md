---
title: "HumeSpeechResult"
description: "Synthesized Hume audio."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeSpeechResult"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeSpeechResult"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeSpeechResult
```


Synthesized Hume audio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L147)

## Constructors

<a id="Vercel_AI_Hume_HumeSpeechResult__ctor_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResult__ctor_System_Byte___System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Hume_HumeWarning__Vercel_AI_Hume_HumeSpeechResponse_"></a>

### `HumeSpeechResult(byte[], string, IReadOnlyList<HumeWarning>, HumeSpeechResponse)`


```csharp
public HumeSpeechResult(byte[] audio, string mediaType, IReadOnlyList<HumeWarning> warnings, HumeSpeechResponse response)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;HumeWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `response` | [HumeSpeechResponse](/api/Vercel.AI.Hume/HumeSpeechResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L150)

## Properties

<a id="Vercel_AI_Hume_HumeSpeechResult_Audio_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L159)

<a id="Vercel_AI_Hume_HumeSpeechResult_MediaType_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Response media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L162)

<a id="Vercel_AI_Hume_HumeSpeechResult_Response_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResult_Response"></a>

### Response


```csharp
public HumeSpeechResponse Response { get; }
```


Response metadata.

**Value:** [HumeSpeechResponse](/api/Vercel.AI.Hume/HumeSpeechResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L168)

<a id="Vercel_AI_Hume_HumeSpeechResult_Warnings_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<HumeWarning> Warnings { get; }
```


Warnings for ignored settings.

**Value:** [IReadOnlyList&lt;HumeWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L165)
