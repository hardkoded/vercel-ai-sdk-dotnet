---
title: "MistralRealtimeSttOptions"
description: "Speech recognition settings of the managed voice agent. Maps to stt."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeSttOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeSttOptions
```


Speech recognition settings of the managed voice agent. Maps to `stt`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L32)

## Properties

<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_ContextBias_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_ContextBias"></a>

### ContextBias


```csharp
public IReadOnlyList<string>? ContextBias { get; init; }
```


Upstream `contextBias`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L41)

<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_Language_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_Language"></a>

### Language


```csharp
public string? Language { get; init; }
```


Upstream `language`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L38)

<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_Model_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_Model"></a>

### Model


```csharp
public string? Model { get; init; }
```


Upstream `model`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L35)

<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_TargetStreamingDelayMs_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_TargetStreamingDelayMs"></a>

### TargetStreamingDelayMs


```csharp
public int? TargetStreamingDelayMs { get; init; }
```


Upstream `targetStreamingDelayMs`, 80 to 1200.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L44)

<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_TurnDetection_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSttOptions_TurnDetection"></a>

### TurnDetection


```csharp
public MistralRealtimeTurnDetection? TurnDetection { get; init; }
```


Upstream `turnDetection`.

**Value:** [MistralRealtimeTurnDetection](/api/Vercel.AI.Mistral/MistralRealtimeTurnDetection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L47)
