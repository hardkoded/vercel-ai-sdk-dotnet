---
title: "ITelemetry"
description: "Implement this interface to create custom telemetry integrations. A callback is optional: return null from a property to skip it. Maps to Telemetry in teleme..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ITelemetry"
  hidden: true
---

<a id="Vercel_AI_ITelemetry"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public interface ITelemetry
```


Implement this interface to create custom telemetry integrations. A callback is optional: return `null` from a property
to skip it. Maps to `Telemetry` in `telemetry/telemetry.ts`. Only `onStart`, `onObjectStepStart`, `onObjectStepEnd` and the text generation callbacks ( `onStepStart`, `onLanguageModelCallStart`,
`onLanguageModelCallEnd`, `onToolExecutionStart`, `onToolExecutionEnd`, `onStepEnd`, `onStepFinish`, `onEnd`,
`onAbort` and `onError`) and the extract and decide callbacks are ported so far.

**Derived:** [OpenTelemetry](/api/Vercel.AI.OpenTelemetry/OpenTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L16)
