---
title: "MistralRealtimeTurnOptions"
description: "Interruptions and reminders. Maps to turn."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeTurnOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeTurnOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeTurnOptions
```


Interruptions and reminders. Maps to `turn`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L77)

## Properties

<a id="Vercel_AI_Mistral_MistralRealtimeTurnOptions_AllowInterruptions_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTurnOptions_AllowInterruptions"></a>

### AllowInterruptions


```csharp
public bool? AllowInterruptions { get; init; }
```


Upstream `allowInterruptions`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L80)

<a id="Vercel_AI_Mistral_MistralRealtimeTurnOptions_Inactivity_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTurnOptions_Inactivity"></a>

### Inactivity


```csharp
public MistralRealtimeInactivityOptions? Inactivity { get; init; }
```


Upstream `inactivity`.

**Value:** [MistralRealtimeInactivityOptions](/api/Vercel.AI.Mistral/MistralRealtimeInactivityOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L83)
