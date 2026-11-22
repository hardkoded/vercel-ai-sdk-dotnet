---
title: "TelemetryRegistry"
description: "Globally registered telemetry integrations. Maps to telemetry-registry.ts and globalThis.AISDKTELEMETRYINTEGRATIONS."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryRegistry"
  hidden: true
---

<a id="Vercel_AI_TelemetryRegistry"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class TelemetryRegistry
```


Globally registered telemetry integrations. Maps to `telemetry-registry.ts` and
`globalThis.AI_SDK_TELEMETRY_INTEGRATIONS`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryRegistry.cs#L11)

## Properties

<a id="Vercel_AI_TelemetryRegistry_Integrations_"></a>
<a id="Vercel_AI_TelemetryRegistry_Integrations"></a>

### Integrations


```csharp
public static IReadOnlyList<ITelemetry>? Integrations { get; set; }
```


The globally registered integrations (`AI_SDK_TELEMETRY_INTEGRATIONS`). Set it to `null` to clear them.
Prefer `ITelemetry%5b%5d)`.

**Value:** [IReadOnlyList&lt;ITelemetry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryRegistry.cs#L20)

## Methods

<a id="Vercel_AI_TelemetryRegistry_GetGlobalTelemetryIntegrations_"></a>
<a id="Vercel_AI_TelemetryRegistry_GetGlobalTelemetryIntegrations"></a>

### GetGlobalTelemetryIntegrations()


```csharp
public static IReadOnlyList<ITelemetry> GetGlobalTelemetryIntegrations()
```


Returns the globally registered integrations. Maps to `getGlobalTelemetryIntegrations`.

**Returns:** [IReadOnlyList&lt;ITelemetry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryRegistry.cs#L53)

<a id="Vercel_AI_TelemetryRegistry_RegisterTelemetry_"></a>
<a id="Vercel_AI_TelemetryRegistry_RegisterTelemetry_Vercel_AI_ITelemetry___"></a>

### RegisterTelemetry(params ITelemetry[])


```csharp
public static void RegisterTelemetry(params ITelemetry[] integrations)
```


Registers one or more telemetry integrations globally. Maps to `registerTelemetry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `integrations` | `ITelemetry\[\]` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryRegistry.cs#L39)
