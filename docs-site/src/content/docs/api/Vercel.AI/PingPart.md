---
title: "PingPart"
description: "A transient provider heartbeat. It contains no generated content, does not reset model-output timeouts, and is omitted from UI message streams. Providers may..."
editUrl: false
prev: false
next: false
sidebar:
  label: "PingPart"
  hidden: true
---

<a id="Vercel_AI_PingPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class PingPart : TextStreamPart
```


A transient provider heartbeat. It contains no generated content, does not reset model-output timeouts, and is omitted from UI message streams.
Providers may require an explicit opt-in. Maps to `TextStreamPingPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1308)

## Constructors

<a id="Vercel_AI_PingPart__ctor_"></a>
<a id="Vercel_AI_PingPart__ctor"></a>

### PingPart()


```csharp
public PingPart()
```


Creates a ping part.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1311)
