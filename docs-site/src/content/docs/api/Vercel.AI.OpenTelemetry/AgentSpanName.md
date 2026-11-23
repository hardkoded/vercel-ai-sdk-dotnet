---
title: "AgentSpanName"
description: "Controls the root agent span names. Maps to the agentSpanName option of OpenTelemetryOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "AgentSpanName"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_AgentSpanName"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class AgentSpanName
```


Controls the root agent span names. Maps to the `agentSpanName` option of `OpenTelemetryOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L63)

## Properties

<a id="Vercel_AI_OpenTelemetry_AgentSpanName_FunctionId_"></a>
<a id="Vercel_AI_OpenTelemetry_AgentSpanName_FunctionId"></a>

### FunctionId


```csharp
public static AgentSpanName FunctionId { get; }
```


Names are `invoke_agent {functionId}`, falling back to the model id. Maps to `'functionId'`.

**Value:** [AgentSpanName](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L78)

<a id="Vercel_AI_OpenTelemetry_AgentSpanName_ModelId_"></a>
<a id="Vercel_AI_OpenTelemetry_AgentSpanName_ModelId"></a>

### ModelId


```csharp
public static AgentSpanName ModelId { get; }
```


Names are `invoke_agent {modelId}`. The default. Maps to `'modelId'`.

**Value:** [AgentSpanName](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L75)

## Methods

<a id="Vercel_AI_OpenTelemetry_AgentSpanName_From_"></a>
<a id="Vercel_AI_OpenTelemetry_AgentSpanName_From_Vercel_AI_OpenTelemetry_AgentSpanNameCallback_"></a>

### From(AgentSpanNameCallback)


```csharp
public static AgentSpanName From(AgentSpanNameCallback callback)
```


Returns the complete span name. A null result or an exception falls back to the default name.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callback` | [AgentSpanNameCallback](/api/Vercel.AI.OpenTelemetry/AgentSpanNameCallback/) |  |

**Returns:** [AgentSpanName](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L81)
