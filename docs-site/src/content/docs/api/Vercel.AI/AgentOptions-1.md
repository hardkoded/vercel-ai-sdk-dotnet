---
title: "AgentOptions<TCallOptions>"
description: "Agent settings with call options. Maps to ToolLoopAgentSettings with a CALLOPTIONS type."
editUrl: false
prev: false
next: false
sidebar:
  label: "AgentOptions<TCallOptions>"
  hidden: true
---

<a id="Vercel_AI_AgentOptions_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class AgentOptions<TCallOptions> : AgentOptions
```


Agent settings with call options. Maps to `ToolLoopAgentSettings` with a `CALL_OPTIONS` type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TCallOptions` | The type of the per-call options. Maps to `CALL_OPTIONS`. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AgentOptions](/api/Vercel.AI/AgentOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1044)

## Properties

<a id="Vercel_AI_AgentOptions_1_CallOptionsSchema_"></a>
<a id="Vercel_AI_AgentOptions_1_CallOptionsSchema"></a>

### CallOptionsSchema


```csharp
public JsonNode? CallOptionsSchema { get; set; }
```


The JSON schema of the call options. Each call validates its options against it before `PrepareCall` runs
and throws a [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/) (context field `options`) on a mismatch. Maps to `callOptionsSchema`.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1050)

<a id="Vercel_AI_AgentOptions_1_PrepareCall_"></a>
<a id="Vercel_AI_AgentOptions_1_PrepareCall"></a>

### PrepareCall


```csharp
public Func<GenerateTextOptions, TCallOptions, GenerateTextOptions?>? PrepareCall { get; set; }
```


Called before each call with the options the agent would use and the validated call options. Return replacement options, or null to keep them.
It replaces [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall), which `Agent%601` ignores. Maps to `prepareCall`.

**Value:** [Func&lt;GenerateTextOptions, TCallOptions, GenerateTextOptions&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1056)
