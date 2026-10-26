---
title: "RevAIOptions"
description: "Rev.ai settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAIOptions"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAIOptions"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public class RevAIOptions : OpenAICompatibleOptions
```


Rev.ai settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L17)

## Properties

<a id="Vercel_AI_RevAI_RevAIOptions_Clock_"></a>
<a id="Vercel_AI_RevAI_RevAIOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps. Defaults to UTC now.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L20)

<a id="Vercel_AI_RevAI_RevAIOptions_PollDelay_"></a>
<a id="Vercel_AI_RevAI_RevAIOptions_PollDelay"></a>

### PollDelay


```csharp
public Func<CancellationToken, Task>? PollDelay { get; set; }
```


Wait between job status polls. Defaults to one second.

**Value:** [Func&lt;CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L23)
