---
title: "EnrichSpan"
description: "Adds custom attributes to a span when it is created. Return null to add none. An exception is ignored. Maps to EnrichSpan."
editUrl: false
prev: false
next: false
sidebar:
  label: "EnrichSpan"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_EnrichSpan"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public delegate IReadOnlyDictionary<string, object?>? EnrichSpan(EnrichSpanContext context)
```


Adds custom attributes to a span when it is created. Return null to add none. An exception is ignored.
Maps to `EnrichSpan`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `context` | [EnrichSpanContext](/api/Vercel.AI.OpenTelemetry/EnrichSpanContext/) |  |

**Returns:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L51)
