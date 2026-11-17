---
title: "RealtimeModelV4Usage"
description: "Normalized realtime usage on completion events or provider custom snapshots. Maps to RealtimeModelV4Usage. Snapshots without a response association must not..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4Usage"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4Usage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4Usage
```


Normalized realtime usage on completion events or provider custom snapshots. Maps to
`RealtimeModelV4Usage`. Snapshots without a response association must not be summed as increments.
Counts are the provider's raw counts: input buckets include cached tokens, and the cached portions are reported
separately. Every field is optional; the whole usage is omitted when nothing is observable.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L13)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_CachedInputAudioTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_CachedInputAudioTokens"></a>

### CachedInputAudioTokens


```csharp
public int? CachedInputAudioTokens { get; set; }
```


Cached portion of the input audio tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L40)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_CachedInputTextTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_CachedInputTextTokens"></a>

### CachedInputTextTokens


```csharp
public int? CachedInputTextTokens { get; set; }
```


Cached portion of the input text tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L37)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_CachedInputTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_CachedInputTokens"></a>

### CachedInputTokens


```csharp
public int? CachedInputTokens { get; set; }
```


Cached portion of gross input tokens, when the provider reports a total.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L22)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_InputAudioTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_InputAudioTokens"></a>

### InputAudioTokens


```csharp
public int? InputAudioTokens { get; set; }
```


Input audio tokens (includes any cached audio tokens).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L28)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_InputTextTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_InputTextTokens"></a>

### InputTextTokens


```csharp
public int? InputTextTokens { get; set; }
```


Input text tokens (includes any cached text tokens).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L25)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_InputTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; set; }
```


Gross input tokens across all reported modalities; includes cached tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L16)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_OutputAudioTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_OutputAudioTokens"></a>

### OutputAudioTokens


```csharp
public int? OutputAudioTokens { get; set; }
```


Output audio tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L34)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_OutputTextTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_OutputTextTokens"></a>

### OutputTextTokens


```csharp
public int? OutputTextTokens { get; set; }
```


Output text tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L31)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_OutputTokens_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; set; }
```


Gross output tokens across all reported modalities.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L19)

<a id="Vercel_AI_Provider_RealtimeModelV4Usage_Seconds_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4Usage_Seconds"></a>

### Seconds


```csharp
public double? Seconds { get; set; }
```


Audio duration in seconds. Populated for duration-billed transcription.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Usage.cs#L43)
