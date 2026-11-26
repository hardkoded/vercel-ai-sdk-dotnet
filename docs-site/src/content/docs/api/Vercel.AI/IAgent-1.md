---
title: "IAgent<TCallOptions>"
description: "An agent that streams a reply to model messages and takes call options. Maps to the Agent interface with a CALLOPTIONS type."
editUrl: false
prev: false
next: false
sidebar:
  label: "IAgent<TCallOptions>"
  hidden: true
---

<a id="Vercel_AI_IAgent_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public interface IAgent<TCallOptions>
```


An agent that streams a reply to model messages and takes call options. Maps to the `Agent` interface with a `CALL_OPTIONS` type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TCallOptions` | The type of the per-call options. Maps to `CALL_OPTIONS`. |

**Derived:** [Agent&lt;TCallOptions&gt;](/api/Vercel.AI/Agent-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L43)
