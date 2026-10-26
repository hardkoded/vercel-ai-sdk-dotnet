---
title: "OpenTelemetryServiceCollectionExtensions"
description: "Registers OpenTelemetryAiTelemetry."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetryServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public static class OpenTelemetryServiceCollectionExtensions
```


Registers [OpenTelemetryAiTelemetry](/api/Vercel.AI.OpenTelemetry/OpenTelemetryAiTelemetry/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L107)

## Methods

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryServiceCollectionExtensions_AddAiSdkOpenTelemetry_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryServiceCollectionExtensions_AddAiSdkOpenTelemetry_Microsoft_Extensions_DependencyInjection_IServiceCollection_"></a>

### AddAiSdkOpenTelemetry(IServiceCollection)


```csharp
public static IServiceCollection AddAiSdkOpenTelemetry(this IServiceCollection services)
```


Registers [IAiTelemetry](/api/Vercel.AI/IAiTelemetry/) so `AddAiSdk` can attach spans.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L110)
