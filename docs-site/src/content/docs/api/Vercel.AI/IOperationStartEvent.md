---
title: "IOperationStartEvent"
description: "An event passed to OnStart when an operation begins. Stands in for the OperationStartEvent union in telemetry/telemetry.ts, which C# cannot express."
editUrl: false
prev: false
next: false
sidebar:
  label: "IOperationStartEvent"
  hidden: true
---

<a id="Vercel_AI_IOperationStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public interface IOperationStartEvent
```


An event passed to [OnStart](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnStart) when an operation begins. Stands in for the `OperationStartEvent` union in
`telemetry/telemetry.ts`, which C# cannot express.

**Derived:** [GenerateObjectStartEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStartEvent/), [GenerateTextStartEvent](/api/Vercel.AI/GenerateTextStartEvent/), [EmbedStartEvent](/api/Vercel.AI.Operations/EmbedStartEvent/), [RerankStartEvent](/api/Vercel.AI.Operations/RerankStartEvent/), [TranscriptionStartEvent](/api/Vercel.AI.Operations/TranscriptionStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L229)
