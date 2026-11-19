---
title: "CreateAgentUIStreamOptions<TCallOptions>"
description: "Options of CancellationToken). Maps to the options of createAgentUIStream with a CALLOPTIONS type."
editUrl: false
prev: false
next: false
sidebar:
  label: "CreateAgentUIStreamOptions<TCallOptions>"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_1"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public class CreateAgentUIStreamOptions<TCallOptions> : CreateAgentUIStreamOptionsBase
```


Options of `CancellationToken)`. Maps to the options of `createAgentUIStream` with a `CALL_OPTIONS` type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TCallOptions` | The type of the call options of the agent. Maps to `CALL_OPTIONS`. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [CreateAgentUIStreamOptionsBase](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptionsBase/)

**Derived:** [CreateAgentUIStreamResponseOptions&lt;TCallOptions&gt;](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamResponseOptions-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L73)

## Properties

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_1_Agent_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_1_Agent"></a>

### Agent


```csharp
public IAgent<TCallOptions>? Agent { get; set; }
```


The agent to run. Maps to `agent`.

**Value:** `IAgent&lt;TCallOptions&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L76)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_1_Options_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamOptions_1_Options"></a>

### Options


```csharp
public TCallOptions? Options { get; set; }
```


The options of the agent call. Maps to `options`.

**Value:** `TCallOptions`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStream.cs#L79)
