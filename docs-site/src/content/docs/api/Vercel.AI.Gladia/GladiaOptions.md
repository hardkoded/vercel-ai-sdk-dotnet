---
title: "GladiaOptions"
description: "Gladia settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaOptions"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaOptions"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public class GladiaOptions : OpenAICompatibleOptions
```


Gladia settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L39)

## Properties

<a id="Vercel_AI_Gladia_GladiaOptions_Clock_"></a>
<a id="Vercel_AI_Gladia_GladiaOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps and to measure the polling budget.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L42)

<a id="Vercel_AI_Gladia_GladiaOptions_PollDelay_"></a>
<a id="Vercel_AI_Gladia_GladiaOptions_PollDelay"></a>

### PollDelay


```csharp
public Func<CancellationToken, Task>? PollDelay { get; set; }
```


Wait between result polls. Defaults to [PollIntervalMs](#Vercel_AI_Gladia_GladiaOptions_PollIntervalMs).

**Value:** [Func&lt;CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L45)

<a id="Vercel_AI_Gladia_GladiaOptions_PollIntervalMs_"></a>
<a id="Vercel_AI_Gladia_GladiaOptions_PollIntervalMs"></a>

### PollIntervalMs


```csharp
public int? PollIntervalMs { get; set; }
```


Interval in milliseconds between transcription job status checks. Defaults to 1000 (1 second).
Must be a positive integer. A per-call [PollIntervalMs](/api/Vercel.AI.Gladia/GladiaTranscriptionRequest/#Vercel_AI_Gladia_GladiaTranscriptionRequest_PollIntervalMs) overrides it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L51)

<a id="Vercel_AI_Gladia_GladiaOptions_PollTimeoutMs_"></a>
<a id="Vercel_AI_Gladia_GladiaOptions_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
public int? PollTimeoutMs { get; set; }
```


Maximum time in milliseconds to wait for an asynchronous transcription job to finish before throwing
a `TranscriptionJobPollingTimedOut` error. Defaults to 600000 (10 minutes). Must be a positive integer.
A per-call [PollTimeoutMs](/api/Vercel.AI.Gladia/GladiaTranscriptionRequest/#Vercel_AI_Gladia_GladiaTranscriptionRequest_PollTimeoutMs) overrides it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L58)
