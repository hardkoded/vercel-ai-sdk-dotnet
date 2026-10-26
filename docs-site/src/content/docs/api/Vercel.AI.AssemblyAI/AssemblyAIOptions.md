---
title: "AssemblyAIOptions"
description: "AssemblyAI settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIOptions"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIOptions"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public class AssemblyAIOptions : OpenAICompatibleOptions
```


AssemblyAI settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L18)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIOptions_Clock_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps. Defaults to UTC now.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L21)

<a id="Vercel_AI_AssemblyAI_AssemblyAIOptions_PollDelay_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIOptions_PollDelay"></a>

### PollDelay


```csharp
public Func<CancellationToken, Task>? PollDelay { get; set; }
```


Wait between transcript status polls. Defaults to three seconds.

**Value:** [Func&lt;CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L24)
