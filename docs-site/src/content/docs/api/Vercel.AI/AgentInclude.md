---
title: "AgentInclude"
description: "The include setting of AgentOptions. Upstream types it as GenerateTextInclude & StreamTextInclude, which C# cannot express, so it extends GenerateTextInclude..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AgentInclude"
  hidden: true
---

<a id="Vercel_AI_AgentInclude"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class AgentInclude : GenerateTextInclude
```


The `include` setting of [AgentOptions](/api/Vercel.AI/AgentOptions/). Upstream types it as `GenerateTextInclude & StreamTextInclude`, which C# cannot express,
so it extends [GenerateTextInclude](/api/Vercel.AI/GenerateTextInclude/) with the [RawChunks](#Vercel_AI_AgentInclude_RawChunks) of [StreamTextInclude](/api/Vercel.AI/StreamTextInclude/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateTextInclude](/api/Vercel.AI/GenerateTextInclude/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L645)

## Properties

<a id="Vercel_AI_AgentInclude_RawChunks_"></a>
<a id="Vercel_AI_AgentInclude_RawChunks"></a>

### RawChunks


```csharp
public bool RawChunks { get; init; }
```


Whether `Stream` forwards the provider's raw chunks. See [RawChunks](/api/Vercel.AI/StreamTextInclude/#Vercel_AI_StreamTextInclude_RawChunks). Maps to `rawChunks`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L648)
