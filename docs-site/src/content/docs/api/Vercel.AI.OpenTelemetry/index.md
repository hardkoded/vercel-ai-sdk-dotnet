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
| [AgentSpanName](/api/Vercel.AI.OpenTelemetry/AgentSpanName/) | Controls the root agent span names. Maps to the agentSpanName option of OpenTelemetryOptions. |
| [EnrichSpanContext](/api/Vercel.AI.OpenTelemetry/EnrichSpanContext/) | The call an EnrichSpan callback is asked about. Maps to the callback options in EnrichSpan. |
| [GenAiConventions](/api/Vercel.AI.OpenTelemetry/GenAiConventions/) | GenAI semantic-convention names shared by spans and the UI message protocol. |
| [OpenTelemetry](/api/Vercel.AI.OpenTelemetry/OpenTelemetry/) | Telemetry integration that records GenAI spans on an ActivitySource. Maps to OpenTelemetry in packages/otel/src/open-telemetry.ts. Pass it in Integrations or register it with ITelemetry%5b%5d). |
| [OpenTelemetryAiTelemetry](/api/Vercel.AI.OpenTelemetry/OpenTelemetryAiTelemetry/) | IAiTelemetry that records spans on ActivitySource. |
| [OpenTelemetryOptions](/api/Vercel.AI.OpenTelemetry/OpenTelemetryOptions/) | Options of OpenTelemetry. Maps to OpenTelemetryOptions. |
| [OpenTelemetryServiceCollectionExtensions](/api/Vercel.AI.OpenTelemetry/OpenTelemetryServiceCollectionExtensions/) | Registers OpenTelemetryAiTelemetry. |
| [ResolvableTelemetryAttribute](/api/Vercel.AI.OpenTelemetry/ResolvableTelemetryAttribute/) | An attribute whose value is computed only when inputs or outputs are recorded. |
| [TelemetryAttributes](/api/Vercel.AI.OpenTelemetry/TelemetryAttributes/) | Selects and sanitizes OpenTelemetry attribute values. |
| [TelemetrySelection](/api/Vercel.AI.OpenTelemetry/TelemetrySelection/) | Which recorded attributes a telemetry call should keep. |
| [TelemetrySpan](/api/Vercel.AI.OpenTelemetry/TelemetrySpan/) | Starts an Activity span and records errors on it. |

## Enums

| Type | Summary |
| --- | --- |
| [OpenTelemetrySpanType](/api/Vercel.AI.OpenTelemetry/OpenTelemetrySpanType/) | The kind of span an EnrichSpan callback is asked about. Maps to OpenTelemetrySpanType. |

## Delegates

| Type | Summary |
| --- | --- |
| [AgentSpanNameCallback](/api/Vercel.AI.OpenTelemetry/AgentSpanNameCallback/) | Maps to the agentSpanName callback of OpenTelemetryOptions. Return null for the default name. |
| [EnrichSpan](/api/Vercel.AI.OpenTelemetry/EnrichSpan/) | Adds custom attributes to a span when it is created. Return null to add none. An exception is ignored. Maps to EnrichSpan. |
