---
title: "RerankTelemetry"
description: "Telemetry callbacks for rerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankTelemetry"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankTelemetry"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankTelemetry
```


Telemetry callbacks for rerank.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L319)

## Properties

<a id="Vercel_AI_Operations_RerankTelemetry_IncludeRuntimeContext_"></a>
<a id="Vercel_AI_Operations_RerankTelemetry_IncludeRuntimeContext"></a>

### IncludeRuntimeContext


```csharp
public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }
```


Runtime context keys included in telemetry.

**Value:** [IReadOnlyDictionary&lt;string, bool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L337)

<a id="Vercel_AI_Operations_RerankTelemetry_OnEnd_"></a>
<a id="Vercel_AI_Operations_RerankTelemetry_OnEnd"></a>

### OnEnd


```csharp
public Func<RerankEndEvent, Task>? OnEnd { get; set; }
```


Operation end.

**Value:** [Func&lt;RerankEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L325)

<a id="Vercel_AI_Operations_RerankTelemetry_OnError_"></a>
<a id="Vercel_AI_Operations_RerankTelemetry_OnError"></a>

### OnError


```csharp
public Func<string, Exception, Task>? OnError { get; set; }
```


Failure callback.

**Value:** [Func&lt;string, Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L334)

<a id="Vercel_AI_Operations_RerankTelemetry_OnRerankEnd_"></a>
<a id="Vercel_AI_Operations_RerankTelemetry_OnRerankEnd"></a>

### OnRerankEnd


```csharp
public Func<RerankModelCallEvent, Task>? OnRerankEnd { get; set; }
```


Inner `doRerank` end.

**Value:** [Func&lt;RerankModelCallEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L331)

<a id="Vercel_AI_Operations_RerankTelemetry_OnRerankStart_"></a>
<a id="Vercel_AI_Operations_RerankTelemetry_OnRerankStart"></a>

### OnRerankStart


```csharp
public Func<RerankModelCallEvent, Task>? OnRerankStart { get; set; }
```


Inner `doRerank` start.

**Value:** [Func&lt;RerankModelCallEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L328)

<a id="Vercel_AI_Operations_RerankTelemetry_OnStart_"></a>
<a id="Vercel_AI_Operations_RerankTelemetry_OnStart"></a>

### OnStart


```csharp
public Func<RerankStartEvent, Task>? OnStart { get; set; }
```


Operation start.

**Value:** [Func&lt;RerankStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L322)
