---
title: "Vercel.AI.OpenTelemetry"
description: "Public types in the `Vercel.AI.OpenTelemetry` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.OpenTelemetry"
---

[API reference](/api/)

Public types in the `Vercel.AI.OpenTelemetry` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [GenAiConventions](/api/Vercel.AI.OpenTelemetry/GenAiConventions/) | GenAI semantic-convention names shared by spans and the UI message protocol. |
| [OpenTelemetryAiTelemetry](/api/Vercel.AI.OpenTelemetry/OpenTelemetryAiTelemetry/) | IAiTelemetry that records spans on ActivitySource. |
| [OpenTelemetryServiceCollectionExtensions](/api/Vercel.AI.OpenTelemetry/OpenTelemetryServiceCollectionExtensions/) | Registers OpenTelemetryAiTelemetry. |
| [ResolvableTelemetryAttribute](/api/Vercel.AI.OpenTelemetry/ResolvableTelemetryAttribute/) | An attribute whose value is computed only when inputs or outputs are recorded. |
| [TelemetryAttributes](/api/Vercel.AI.OpenTelemetry/TelemetryAttributes/) | Selects and sanitizes OpenTelemetry attribute values. |
| [TelemetrySelection](/api/Vercel.AI.OpenTelemetry/TelemetrySelection/) | Which recorded attributes a telemetry call should keep. |
| [TelemetrySpan](/api/Vercel.AI.OpenTelemetry/TelemetrySpan/) | Starts an Activity span and records errors on it. |
