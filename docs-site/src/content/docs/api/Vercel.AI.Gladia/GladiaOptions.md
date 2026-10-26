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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L16)

## Properties

<a id="Vercel_AI_Gladia_GladiaOptions_Clock_"></a>
<a id="Vercel_AI_Gladia_GladiaOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L19)

<a id="Vercel_AI_Gladia_GladiaOptions_PollDelay_"></a>
<a id="Vercel_AI_Gladia_GladiaOptions_PollDelay"></a>

### PollDelay


```csharp
public Func<CancellationToken, Task>? PollDelay { get; set; }
```


Wait between result polls. Defaults to one second.

**Value:** [Func&lt;CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L22)
