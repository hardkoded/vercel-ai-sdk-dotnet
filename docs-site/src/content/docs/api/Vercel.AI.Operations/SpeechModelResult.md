---
title: "SpeechModelResult"
description: "Speech model response."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechModelResult
```


Speech model response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L55)

## Constructors

<a id="Vercel_AI_Operations_SpeechModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechModelResult__ctor_System_Byte___System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `SpeechModelResult(byte[]?, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?)`


```csharp
public SpeechModelResult(byte[]? audio, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L58)

## Properties

<a id="Vercel_AI_Operations_SpeechModelResult_Audio_"></a>
<a id="Vercel_AI_Operations_SpeechModelResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes. Empty when the provider returned none.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L67)

<a id="Vercel_AI_Operations_SpeechModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_SpeechModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L73)

<a id="Vercel_AI_Operations_SpeechModelResult_Response_"></a>
<a id="Vercel_AI_Operations_SpeechModelResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L76)

<a id="Vercel_AI_Operations_SpeechModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_SpeechModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L70)
