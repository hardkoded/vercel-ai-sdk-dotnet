---
title: "TelemetryOptions"
description: "Telemetry configuration. Maps to TelemetryOptions in telemetry/telemetry-options.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryOptions"
  hidden: true
---

<a id="Vercel_AI_TelemetryOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TelemetryOptions
```


Telemetry configuration. Maps to `TelemetryOptions` in `telemetry/telemetry-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L8)

## Properties

<a id="Vercel_AI_TelemetryOptions_FunctionId_"></a>
<a id="Vercel_AI_TelemetryOptions_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; set; }
```


Identifier for this function. Used to group telemetry data by function. Maps to `functionId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L23)

<a id="Vercel_AI_TelemetryOptions_IncludeRuntimeContext_"></a>
<a id="Vercel_AI_TelemetryOptions_IncludeRuntimeContext"></a>

### IncludeRuntimeContext


```csharp
public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }
```


Top-level runtime context properties that should be included in telemetry. Runtime context properties are excluded
unless they are explicitly set to `true`. Maps to `includeRuntimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, bool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L35)

<a id="Vercel_AI_TelemetryOptions_IncludeToolsContext_"></a>
<a id="Vercel_AI_TelemetryOptions_IncludeToolsContext"></a>

### IncludeToolsContext


```csharp
public IReadOnlyDictionary<string, IReadOnlyDictionary<string, bool>>? IncludeToolsContext { get; set; }
```


Top-level tool context properties that should be included in telemetry, configured per tool name. Tool context properties
are excluded unless they are explicitly set to `true`. Maps to `includeToolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyDictionary&lt;string, bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L41)

<a id="Vercel_AI_TelemetryOptions_Integrations_"></a>
<a id="Vercel_AI_TelemetryOptions_Integrations"></a>

### Integrations


```csharp
public IReadOnlyList<ITelemetry>? Integrations { get; set; }
```


Per-call telemetry integrations that receive lifecycle events during generation. When provided, they take precedence over
the globally registered integrations for this call. Maps to `integrations`.

**Value:** [IReadOnlyList&lt;ITelemetry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L29)

<a id="Vercel_AI_TelemetryOptions_IsEnabled_"></a>
<a id="Vercel_AI_TelemetryOptions_IsEnabled"></a>

### IsEnabled


```csharp
public bool? IsEnabled { get; set; }
```


Enable or disable telemetry. Enabled by default when a telemetry integration is registered. Set to
`false` to opt out. Maps to `isEnabled`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L14)

<a id="Vercel_AI_TelemetryOptions_RecordInputs_"></a>
<a id="Vercel_AI_TelemetryOptions_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; set; }
```


Enable or disable input recording. Enabled by default. Maps to `recordInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L17)

<a id="Vercel_AI_TelemetryOptions_RecordOutputs_"></a>
<a id="Vercel_AI_TelemetryOptions_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; set; }
```


Enable or disable output recording. Enabled by default. Maps to `recordOutputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryOptions.cs#L20)
