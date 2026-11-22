---
title: "IOperationEndEvent"
description: "An event passed to OnEnd when an operation completes. Stands in for the OperationEndEvent union in telemetry/telemetry.ts, which C# cannot express."
editUrl: false
prev: false
next: false
sidebar:
  label: "IOperationEndEvent"
  hidden: true
---

<a id="Vercel_AI_IOperationEndEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public interface IOperationEndEvent
```


An event passed to [OnEnd](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnEnd) when an operation completes. Stands in for the `OperationEndEvent` union in
`telemetry/telemetry.ts`, which C# cannot express.

**Derived:** [GenerateTextResult](/api/Vercel.AI/GenerateTextResult/), [GenerateTextResult&lt;TOutput&gt;](/api/Vercel.AI/GenerateTextResult-1/), [EmbedEndEvent](/api/Vercel.AI.Operations/EmbedEndEvent/), [RerankEndEvent](/api/Vercel.AI.Operations/RerankEndEvent/), [TranscriptionEndEvent](/api/Vercel.AI.Operations/TranscriptionEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L148)
