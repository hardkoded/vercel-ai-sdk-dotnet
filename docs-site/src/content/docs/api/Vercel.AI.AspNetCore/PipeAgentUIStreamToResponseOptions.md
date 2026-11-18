---
title: "PipeAgentUIStreamToResponseOptions"
description: "Options of CancellationToken). Maps to the options of pipeAgentUIStreamToResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "PipeAgentUIStreamToResponseOptions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_PipeAgentUIStreamToResponseOptions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class PipeAgentUIStreamToResponseOptions : CreateAgentUIStreamResponseOptions
```


Options of `CancellationToken)`. Maps to the options of `pipeAgentUIStreamToResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [CreateAgentUIStreamOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptions/) → [CreateAgentUIStreamResponseOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamResponseOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/PipeAgentUIStreamToResponse.cs#L10)

## Properties

<a id="Vercel_AI_AspNetCore_PipeAgentUIStreamToResponseOptions_Response_"></a>
<a id="Vercel_AI_AspNetCore_PipeAgentUIStreamToResponseOptions_Response"></a>

### Response


```csharp
public ServerResponse? Response { get; set; }
```


The response to pipe to. Maps to `response`.

**Value:** [ServerResponse](/api/Vercel.AI.Util/ServerResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/PipeAgentUIStreamToResponse.cs#L13)
