---
title: "GladiaTranscriptionRequest"
description: "Headers and extra pre-recorded fields for one Gladia transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaTranscriptionRequest"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaTranscriptionRequest
```


Headers and extra pre-recorded fields for one Gladia transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L115)

## Properties

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Body_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Body"></a>

### Body


```csharp
public JsonObject? Body { get; set; }
```


Extra JSON fields sent with `audio_url`, in the snake_case names of the Gladia API. .NET-only: they are
merged over the fields built from [ProviderOptions](#Vercel_AI_Gladia_GladiaTranscriptionRequest_ProviderOptions).

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L127)

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Headers_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L118)

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_PollIntervalMs_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public int? PollIntervalMs { get; set; }
```


Interval in milliseconds between job status checks for this call. Overrides
[PollIntervalMs](/api/Vercel.AI.Gladia/GladiaOptions/#Vercel_AI_Gladia_GladiaOptions_PollIntervalMs). Must be a positive integer. Not sent to the Gladia API.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L133)

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_PollTimeoutMs_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public int? PollTimeoutMs { get; set; }
```


Maximum time in milliseconds to wait for the job to finish for this call. Overrides
[PollTimeoutMs](/api/Vercel.AI.Gladia/GladiaOptions/#Vercel_AI_Gladia_GladiaOptions_PollTimeoutMs). Must be a positive integer. Not sent to the Gladia API.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L139)

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options. The `gladia` object is read, with camelCase keys.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L121)
