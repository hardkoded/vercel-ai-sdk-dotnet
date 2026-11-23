---
title: "AgentSpanNameCallback"
description: "Maps to the agentSpanName callback of OpenTelemetryOptions. Return null for the default name."
editUrl: false
prev: false
next: false
sidebar:
  label: "AgentSpanNameCallback"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_AgentSpanNameCallback"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public delegate string? AgentSpanNameCallback(string modelId, string? functionId)
```


Maps to the `agentSpanName` callback of `OpenTelemetryOptions`. Return null for the default name.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L60)
