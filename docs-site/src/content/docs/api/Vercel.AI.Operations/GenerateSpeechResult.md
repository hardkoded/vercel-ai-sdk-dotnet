---
title: "GenerateSpeechResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeechResult"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateSpeechResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateSpeechResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L125)

## Constructors

<a id="Vercel_AI_Operations_GenerateSpeechResult__ctor_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechResult__ctor_Vercel_AI_Operations_GeneratedAudio_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__System_Text_Json_JsonElement_"></a>

### `GenerateSpeechResult(GeneratedAudio, IReadOnlyList<OperationWarning>, IReadOnlyList<ProviderResponse>, JsonElement)`


```csharp
public GenerateSpeechResult(GeneratedAudio audio, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [GeneratedAudio](/api/Vercel.AI.Operations/GeneratedAudio/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L128)

## Properties

<a id="Vercel_AI_Operations_GenerateSpeechResult_Audio_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechResult_Audio"></a>

### Audio


```csharp
public GeneratedAudio Audio { get; }
```


Generated audio.

**Value:** [GeneratedAudio](/api/Vercel.AI.Operations/GeneratedAudio/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L137)

<a id="Vercel_AI_Operations_GenerateSpeechResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Provider metadata. Empty object when omitted.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L146)

<a id="Vercel_AI_Operations_GenerateSpeechResult_Responses_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechResult_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Provider responses.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L143)

<a id="Vercel_AI_Operations_GenerateSpeechResult_Warnings_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L140)
