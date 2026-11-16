---
title: "RealtimeModelV4TurnDetection"
description: "Turn detection options. Maps to RealtimeModelV4SessionConfig.turnDetection."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4TurnDetection"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4TurnDetection
```


Turn detection options. Maps to `RealtimeModelV4SessionConfig.turnDetection`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L146)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_PrefixPaddingMs_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_PrefixPaddingMs"></a>

### PrefixPaddingMs


```csharp
public int? PrefixPaddingMs { get; set; }
```


Prefix padding in milliseconds.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L158)

<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_SilenceDurationMs_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_SilenceDurationMs"></a>

### SilenceDurationMs


```csharp
public int? SilenceDurationMs { get; set; }
```


Silence duration in milliseconds.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L155)

<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_Threshold_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_Threshold"></a>

### Threshold


```csharp
public double? Threshold { get; set; }
```


Activation threshold.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L152)

<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_Type_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TurnDetection_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


`server-vad`, `semantic-vad` or `disabled`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L149)
