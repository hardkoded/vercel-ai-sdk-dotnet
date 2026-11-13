---
title: "ExtractRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L40)

## Properties

<a id="Vercel_AI_Operations_ExtractRequest_Download_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_Download"></a>

### Download


```csharp
public Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? Download { get; set; }
```


Download function required for URL-backed file state. Maps to `experimental_download`. Use
`DownloadFetch)` with a DNS-protected transport for untrusted URLs. The model's fetch is not used.
Without it a URL file throws [DownloadError](/api/Vercel.AI.Util/DownloadError/).

**Value:** [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L71)

<a id="Vercel_AI_Operations_ExtractRequest_Fields_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_Fields"></a>

### Fields


```csharp
public IReadOnlyDictionary<string, ExtractionField>? Fields { get; set; }
```


Nonempty field map.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L52)

<a id="Vercel_AI_Operations_ExtractRequest_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; set; }
```


Maximum output tokens for supported models. Must be an integer of at least 1 when set.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L58)

<a id="Vercel_AI_Operations_ExtractRequest_Model_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_Model"></a>

### Model


```csharp
public IExtractionCaller? Model { get; set; }
```


An extraction model instance.

**Value:** [IExtractionCaller](/api/Vercel.AI.Operations/IExtractionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L43)

<a id="Vercel_AI_Operations_ExtractRequest_Reasoning_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort for supported models: `provider-default`, `none`, `minimal`, `low`, `medium`, `high`,
`xhigh`, or `max`. Language-model adapters default to `none`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L64)

<a id="Vercel_AI_Operations_ExtractRequest_ReferenceDate_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_ReferenceDate"></a>

### ReferenceDate


```csharp
public string? ReferenceDate { get; set; }
```


`YYYY-MM-DD` date that anchors relative dates and dates without a year.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L55)

<a id="Vercel_AI_Operations_ExtractRequest_State_"></a>
<a id="Vercel_AI_Operations_ExtractRequest_State"></a>

### State


```csharp
public DecisionState? State { get; set; }
```


Shared state: a string, a JSON object, or a list of parts. A JSON array is not valid state.
Wrap it in an object or in a [DecisionJsonPart](/api/Vercel.AI.Operations/DecisionJsonPart/). See [DecisionState](/api/Vercel.AI.Operations/DecisionState/).

**Value:** [DecisionState](/api/Vercel.AI.Operations/DecisionState/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L49)
