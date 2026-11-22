---
title: "AssemblyAISpeechResult"
description: "Synthesized AssemblyAI audio."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAISpeechResult"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAISpeechResult
```


Synthesized AssemblyAI audio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L282)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult__ctor_System_Byte___System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__System_String_Vercel_AI_Provider_AudioUsage_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_"></a>

### `AssemblyAISpeechResult(byte[], string, IReadOnlyList<ModelWarning>, string, AudioUsage?, JsonElement?, AssemblyAISpeechResponse)`


```csharp
public AssemblyAISpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, AudioUsage? usage, JsonElement? providerMetadata, AssemblyAISpeechResponse response)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `requestBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usage` | [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [AssemblyAISpeechResponse](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L285)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Audio_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L305)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_MediaType_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Response media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L308)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_ProviderMetadata_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Response details under `assemblyai`: request id, model, voice, language, encoding, sample rate, duration, billed characters.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L320)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_RequestBody_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_RequestBody"></a>

### RequestBody


```csharp
public string RequestBody { get; }
```


JSON request body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L314)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Response_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Response"></a>

### Response


```csharp
public AssemblyAISpeechResponse Response { get; }
```


Response metadata.

**Value:** [AssemblyAISpeechResponse](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L323)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Usage_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Usage"></a>

### Usage


```csharp
public AudioUsage? Usage { get; }
```


Billed characters, when the response reports them.

**Value:** [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L317)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Warnings_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for ignored settings.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L311)
