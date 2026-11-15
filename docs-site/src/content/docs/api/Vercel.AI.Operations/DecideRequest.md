---
title: "DecideRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecideRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_DecideRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecideRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L463)

## Properties

<a id="Vercel_AI_Operations_DecideRequest_Download_"></a>
<a id="Vercel_AI_Operations_DecideRequest_Download"></a>

### Download


```csharp
public Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? Download { get; set; }
```


Download function required for URL-backed file state. Maps to `experimental_download`. Use
`DownloadFetch)` with a DNS-protected transport for untrusted URLs. The model's fetch is not used.
Without it a URL file throws [DownloadError](/api/Vercel.AI.Util/DownloadError/).

**Value:** [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L498)

<a id="Vercel_AI_Operations_DecideRequest_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_DecideRequest_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; set; }
```


Maximum output tokens for supported models. Must be an integer of at least 1 when set.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L484)

<a id="Vercel_AI_Operations_DecideRequest_Model_"></a>
<a id="Vercel_AI_Operations_DecideRequest_Model"></a>

### Model


```csharp
public IDecisionCaller? Model { get; set; }
```


Explicit decision model. Wins over [ModelId](#Vercel_AI_Operations_DecideRequest_ModelId) when both are set.

**Value:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L466)

<a id="Vercel_AI_Operations_DecideRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_DecideRequest_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Decision model id, used when [Model](#Vercel_AI_Operations_DecideRequest_Model) is null. It goes through [DefaultProvider](/api/Vercel.AI.Operations/DecisionModels/#Vercel_AI_Operations_DecisionModels_DefaultProvider),
or Gateway when none is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L472)

<a id="Vercel_AI_Operations_DecideRequest_OnEnd_"></a>
<a id="Vercel_AI_Operations_DecideRequest_OnEnd"></a>

### OnEnd


```csharp
public Func<DecideEvent, Task>? OnEnd { get; set; }
```


Called when the decision completes. Receives the full runtime context.

**Value:** [Func&lt;DecideEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L504)

<a id="Vercel_AI_Operations_DecideRequest_OnStart_"></a>
<a id="Vercel_AI_Operations_DecideRequest_OnStart"></a>

### OnStart


```csharp
public Func<DecideEvent, Task>? OnStart { get; set; }
```


Called when the decision begins. Receives the full runtime context.

**Value:** [Func&lt;DecideEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L501)

<a id="Vercel_AI_Operations_DecideRequest_Questions_"></a>
<a id="Vercel_AI_Operations_DecideRequest_Questions"></a>

### Questions


```csharp
public IReadOnlyDictionary<string, DecisionQuestion>? Questions { get; set; }
```


Nonempty question map.

**Value:** [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L481)

<a id="Vercel_AI_Operations_DecideRequest_Reasoning_"></a>
<a id="Vercel_AI_Operations_DecideRequest_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort for supported models: `provider-default`, `none`, `minimal`, `low`, `medium`, `high`,
`xhigh`, or `max`. Language-model adapters default to `none`; `provider-default` uses the wrapped language
model's own default instead. Native decision providers return an unsupported-setting warning.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L491)

<a id="Vercel_AI_Operations_DecideRequest_State_"></a>
<a id="Vercel_AI_Operations_DecideRequest_State"></a>

### State


```csharp
public DecisionState? State { get; set; }
```


Shared state: a string, a JSON object, or a list of parts. A JSON array is not valid state.
Wrap it in an object or in a [DecisionJsonPart](/api/Vercel.AI.Operations/DecisionJsonPart/). See [DecisionState](/api/Vercel.AI.Operations/DecisionState/).

**Value:** [DecisionState](/api/Vercel.AI.Operations/DecisionState/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L478)

<a id="Vercel_AI_Operations_DecideRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_DecideRequest_Telemetry"></a>

### Telemetry


```csharp
public DecideTelemetry? Telemetry { get; set; }
```


Telemetry. Generic operation callbacks on the integration are not called.

**Value:** [DecideTelemetry](/api/Vercel.AI.Operations/DecideTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L507)
