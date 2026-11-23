---
title: "OpenTelemetry"
description: "Telemetry integration that records GenAI spans on an ActivitySource. Maps to OpenTelemetry in packages/otel/src/open-telemetry.ts. Pass it in Integrations or..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetry"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class OpenTelemetry : ITelemetry
```


Telemetry integration that records GenAI spans on an [ActivitySource](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitysource). Maps to `OpenTelemetry` in
`packages/otel/src/open-telemetry.ts`. Pass it in [Integrations](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_Integrations) or register it with
`ITelemetry%5b%5d)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITelemetry](/api/Vercel.AI/ITelemetry/)

Only the callbacks [ITelemetry](/api/Vercel.AI/ITelemetry/) has are ported: text generation (root, step, model call, and tool spans), embed,
rerank, speech, transcription, decide, and extract. The object and durable parts of upstream need events
the port does not emit yet.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L23)

## Constructors

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry__ctor_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry__ctor_Vercel_AI_OpenTelemetry_OpenTelemetryOptions_"></a>

### OpenTelemetry(OpenTelemetryOptions?)


```csharp
public OpenTelemetry(OpenTelemetryOptions? options = null)
```


Creates the integration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenTelemetryOptions](/api/Vercel.AI.OpenTelemetry/OpenTelemetryOptions/) | Options. Null uses the defaults. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L39)

## Properties

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_ExecuteLanguageModelCall_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_ExecuteLanguageModelCall"></a>

### ExecuteLanguageModelCall


```csharp
public IExecuteLanguageModelCall? ExecuteLanguageModelCall { get; }
```


Runs the provider call of a model call in an integration-specific context, so auto-instrumented provider requests can be
associated with the model work. Maps to `executeLanguageModelCall`.

**Value:** [IExecuteLanguageModelCall](/api/Vercel.AI/IExecuteLanguageModelCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L372)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_ExecuteTool_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_ExecuteTool"></a>

### ExecuteTool


```csharp
public IExecuteTool? ExecuteTool { get; }
```


Runs the `execute` function of a tool in an integration-specific context, so nested calls become children of the tool
span. Maps to `executeTool`.

**Value:** [IExecuteTool](/api/Vercel.AI/IExecuteTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L375)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnAbort_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnAbort"></a>

### OnAbort


```csharp
public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; }
```


Called when a streamed generation is aborted before it completes. Maps to `onAbort`.

**Value:** [Func&lt;StreamAbortContext, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L138)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecideEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecideEnd"></a>

### OnDecideEnd


```csharp
public Func<DecideEvent, CancellationToken, Task>? OnDecideEnd { get; }
```


Called when an experimental decision completes. Maps to `experimental_onDecideEnd`.

**Value:** [Func&lt;DecideEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L239)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecideStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecideStart"></a>

### OnDecideStart


```csharp
public Func<DecideEvent, CancellationToken, Task>? OnDecideStart { get; }
```


Called when an experimental decision begins. Maps to `experimental_onDecideStart`.

**Value:** [Func&lt;DecideEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L215)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecisionModelCallEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecisionModelCallEnd"></a>

### OnDecisionModelCallEnd


```csharp
public Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallEnd { get; }
```


Called after the decision model response has been validated. Maps to `experimental_onDecisionModelCallEnd`.

**Value:** [Func&lt;DecisionModelEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L231)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecisionModelCallStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnDecisionModelCallStart"></a>

### OnDecisionModelCallStart


```csharp
public Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallStart { get; }
```


Called before the decision model call begins. Maps to `experimental_onDecisionModelCallStart`.

**Value:** [Func&lt;DecisionModelEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L223)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEmbedEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEmbedEnd"></a>

### OnEmbedEnd


```csharp
public Func<EmbeddingModelCallEndEvent, CancellationToken, Task>? OnEmbedEnd { get; }
```


Called after each `doEmbed` call of `embed` and `embedMany`. Maps to `onEmbedEnd`.

**Value:** [Func&lt;EmbeddingModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L310)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEmbedStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEmbedStart"></a>

### OnEmbedStart


```csharp
public Func<EmbeddingModelCallStartEvent, CancellationToken, Task>? OnEmbedStart { get; }
```


Called before each `doEmbed` call of `embed` and `embedMany`. Maps to `onEmbedStart`.

**Value:** [Func&lt;EmbeddingModelCallStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L302)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEnd"></a>

### OnEnd


```csharp
public Func<IOperationEndEvent, CancellationToken, Task>? OnEnd { get; }
```


Called when an operation completes. Maps to `onEnd`. Receives a [GenerateTextResult](/api/Vercel.AI/GenerateTextResult/), a `GenerateObjectEndEvent%601`,
an [EmbedEndEvent](/api/Vercel.AI.Operations/EmbedEndEvent/), a [RerankEndEvent](/api/Vercel.AI.Operations/RerankEndEvent/), a [GenerateSpeechEndEvent](/api/Vercel.AI.Operations/GenerateSpeechEndEvent/) or a [TranscriptionEndEvent](/api/Vercel.AI.Operations/TranscriptionEndEvent/).

**Value:** [Func&lt;IOperationEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L154)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnError_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnError"></a>

### OnError


```csharp
public Func<TelemetryErrorEvent, CancellationToken, Task>? OnError { get; }
```


Called when an unrecoverable error occurs during the generation. Maps to `onError`.

**Value:** [Func&lt;TelemetryErrorEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L146)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluateEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluateEnd"></a>

### OnEvaluateEnd


```csharp
[Obsolete("Use OnDecideEnd instead.")]
public Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateEnd { get; }
```


Deprecated alias of [OnDecideEnd](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnDecideEnd). Maps to `experimental_onEvaluateEnd`.

**Value:** [Func&lt;EvaluateEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L259)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluateStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluateStart"></a>

### OnEvaluateStart


```csharp
[Obsolete("Use OnDecideStart instead.")]
public Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateStart { get; }
```


Deprecated alias of [OnDecideStart](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnDecideStart). Maps to `experimental_onEvaluateStart`.

**Value:** [Func&lt;EvaluateEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L247)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluationModelCallEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluationModelCallEnd"></a>

### OnEvaluationModelCallEnd


```csharp
[Obsolete("Use OnDecisionModelCallEnd instead.")]
public Func<EvaluateModelEvent, CancellationToken, Task>? OnEvaluationModelCallEnd { get; }
```


Deprecated alias of [OnDecisionModelCallEnd](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnDecisionModelCallEnd). Maps to `experimental_onEvaluationModelCallEnd`.

**Value:** [Func&lt;EvaluateModelEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L255)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluationModelCallStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnEvaluationModelCallStart"></a>

### OnEvaluationModelCallStart


```csharp
[Obsolete("Use OnDecisionModelCallStart instead.")]
public Func<EvaluateModelEvent, CancellationToken, Task>? OnEvaluationModelCallStart { get; }
```


Deprecated alias of [OnDecisionModelCallStart](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnDecisionModelCallStart). Maps to `experimental_onEvaluationModelCallStart`.

**Value:** [Func&lt;EvaluateModelEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L251)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractEnd"></a>

### OnExtractEnd


```csharp
public Func<ExtractEndEvent, CancellationToken, Task>? OnExtractEnd { get; }
```


Called when an experimental extraction completes. Maps to `experimental_onExtractEnd`.

**Value:** [Func&lt;ExtractEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L291)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractStart"></a>

### OnExtractStart


```csharp
public Func<ExtractStartEvent, CancellationToken, Task>? OnExtractStart { get; }
```


Called when an experimental extraction begins. Maps to `experimental_onExtractStart`.

**Value:** [Func&lt;ExtractStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L263)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractionModelCallEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractionModelCallEnd"></a>

### OnExtractionModelCallEnd


```csharp
public Func<ExtractionModelCallEndEvent, CancellationToken, Task>? OnExtractionModelCallEnd { get; }
```


Called after the extraction model response has been validated. Maps to `experimental_onExtractionModelCallEnd`.

**Value:** [Func&lt;ExtractionModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L283)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractionModelCallStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnExtractionModelCallStart"></a>

### OnExtractionModelCallStart


```csharp
public Func<ExtractionModelCallStartEvent, CancellationToken, Task>? OnExtractionModelCallStart { get; }
```


Called before the extraction model call begins. Maps to `experimental_onExtractionModelCallStart`.

**Value:** [Func&lt;ExtractionModelCallStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L271)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnLanguageModelCallEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnLanguageModelCallEnd"></a>

### OnLanguageModelCallEnd


```csharp
public Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; }
```


Called after each provider model call returns, before tools run. Maps to `onLanguageModelCallEnd`.

**Value:** [Func&lt;LanguageModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L207)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnLanguageModelCallStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnLanguageModelCallStart"></a>

### OnLanguageModelCallStart


```csharp
public Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; }
```


Called before each provider model call. Maps to `onLanguageModelCallStart`.

**Value:** [Func&lt;LanguageModelCallStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L199)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnObjectStepEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnObjectStepEnd"></a>

### OnObjectStepEnd


```csharp
public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnObjectStepEnd { get; }
```


Called after the model call of `generateObject` and `streamObject`. Maps to `onObjectStepEnd`.

**Value:** [Func&lt;GenerateObjectStepEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L100)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnObjectStepStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnObjectStepStart"></a>

### OnObjectStepStart


```csharp
public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnObjectStepStart { get; }
```


Called before the model call of `generateObject` and `streamObject`. Maps to `onObjectStepStart`.

**Value:** [Func&lt;GenerateObjectStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L97)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnRerankEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnRerankEnd"></a>

### OnRerankEnd


```csharp
public Func<RerankingModelCallEndEvent, CancellationToken, Task>? OnRerankEnd { get; }
```


Called after the `doRerank` call of `rerank`. Maps to `onRerankEnd`.

**Value:** [Func&lt;RerankingModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L326)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnRerankStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnRerankStart"></a>

### OnRerankStart


```csharp
public Func<RerankingModelCallStartEvent, CancellationToken, Task>? OnRerankStart { get; }
```


Called before the `doRerank` call of `rerank`. Maps to `onRerankStart`.

**Value:** [Func&lt;RerankingModelCallStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L318)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStart"></a>

### OnStart


```csharp
public Func<IOperationStartEvent, CancellationToken, Task>? OnStart { get; }
```


Called when an operation begins, before any model call. Maps to `onStart`. Receives a [GenerateTextStartEvent](/api/Vercel.AI/GenerateTextStartEvent/), a [GenerateObjectStartEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStartEvent/), an [EmbedStartEvent](/api/Vercel.AI.Operations/EmbedStartEvent/), a [RerankStartEvent](/api/Vercel.AI.Operations/RerankStartEvent/), a [GenerateSpeechStartEvent](/api/Vercel.AI.Operations/GenerateSpeechStartEvent/) or a [TranscriptionStartEvent](/api/Vercel.AI.Operations/TranscriptionStartEvent/).

**Value:** [Func&lt;IOperationStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L49)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; }
```


Called when a step completes. Maps to `onStepEnd`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L127)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepError_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepError"></a>

### OnStepError


```csharp
public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; }
```


Called when a step ends with an error before it produces a step result. Maps to `onStepError`.

**Value:** [Func&lt;GenerateTextStepErrorEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L299)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepFinish_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepFinish"></a>

### OnStepFinish


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; }
```


Deprecated alias of [OnStepEnd](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnStepEnd). An integration with both gets both calls. Maps to `onStepFinish`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L135)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStepStart"></a>

### OnStepStart


```csharp
public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; }
```


Called when a step begins, before the provider is called. Maps to `onStepStart`.

**Value:** [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L103)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStreamTranscriptionEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStreamTranscriptionEnd"></a>

### OnStreamTranscriptionEnd


```csharp
public Func<StreamTranscriptionEndEvent, CancellationToken, Task>? OnStreamTranscriptionEnd { get; }
```


Called when a stream transcription completes. Maps to `experimental_onStreamTranscriptionEnd`.

**Value:** [Func&lt;StreamTranscriptionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L357)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStreamTranscriptionStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnStreamTranscriptionStart"></a>

### OnStreamTranscriptionStart


```csharp
public Func<StreamTranscriptionStartEvent, CancellationToken, Task>? OnStreamTranscriptionStart { get; }
```


Called when a stream transcription begins. Maps to `experimental_onStreamTranscriptionStart`.

**Value:** [Func&lt;StreamTranscriptionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L334)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnToolExecutionEnd_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnToolExecutionEnd"></a>

### OnToolExecutionEnd


```csharp
public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; }
```


Called when a tool execution completes, with a result or an error. Maps to `onToolExecutionEnd`.

**Value:** [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L119)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnToolExecutionStart_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetry_OnToolExecutionStart"></a>

### OnToolExecutionStart


```csharp
public Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart { get; }
```


Called when a tool execution begins, before the tool runs. Maps to `onToolExecutionStart`.

**Value:** [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetry.cs#L111)
