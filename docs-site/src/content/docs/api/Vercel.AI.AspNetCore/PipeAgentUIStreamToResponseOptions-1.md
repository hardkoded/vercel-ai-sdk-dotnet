---
title: "PipeAgentUIStreamToResponseOptions<TCallOptions>"
description: "Options of CancellationToken). Maps to the options of pipeAgentUIStreamToResponse with a CALLOPTIONS type."
editUrl: false
prev: false
next: false
sidebar:
  label: "PipeAgentUIStreamToResponseOptions<TCallOptions>"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_PipeAgentUIStreamToResponseOptions_1"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class PipeAgentUIStreamToResponseOptions<TCallOptions> : CreateAgentUIStreamResponseOptions<TCallOptions>
```


Options of `CancellationToken)`. Maps to the options of `pipeAgentUIStreamToResponse` with a `CALL_OPTIONS` type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TCallOptions` | The type of the call options of the agent. Maps to `CALL_OPTIONS`. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [CreateAgentUIStreamOptionsBase](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptionsBase/) → `CreateAgentUIStreamOptions&lt;TCallOptions&gt;` → `CreateAgentUIStreamResponseOptions&lt;TCallOptions&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/PipeAgentUIStreamToResponse.cs#L18)

## Properties

<a id="Vercel_AI_AspNetCore_PipeAgentUIStreamToResponseOptions_1_Response_"></a>
<a id="Vercel_AI_AspNetCore_PipeAgentUIStreamToResponseOptions_1_Response"></a>

### Response


```csharp
public ServerResponse? Response { get; set; }
```


The response to pipe to. Maps to `response`.

**Value:** [ServerResponse](/api/Vercel.AI.Util/ServerResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/PipeAgentUIStreamToResponse.cs#L22)
