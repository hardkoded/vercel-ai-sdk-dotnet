---
title: "TelemetrySelection"
description: "Which recorded attributes a telemetry call should keep."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetrySelection"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_TelemetrySelection"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class TelemetrySelection
```


Which recorded attributes a telemetry call should keep.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L10)

## Properties

<a id="Vercel_AI_OpenTelemetry_TelemetrySelection_IsEnabled_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetrySelection_IsEnabled"></a>

### IsEnabled


```csharp
public bool? IsEnabled { get; set; }
```


When false, no attributes are recorded. Null records them.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L13)

<a id="Vercel_AI_OpenTelemetry_TelemetrySelection_RecordInputs_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetrySelection_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; set; }
```


When false, input resolvers are skipped. Null records them.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L16)

<a id="Vercel_AI_OpenTelemetry_TelemetrySelection_RecordOutputs_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetrySelection_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; set; }
```


When false, output resolvers are skipped. Null records them.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L19)
