---
title: "TelemetryRedactedValue"
description: "A JSON value with the provider-owned telemetry view attached. The view is not part of the value, so the original is untouched. Maps to a value annotated by w..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryRedactedValue"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TelemetryRedactedValue"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class TelemetryRedactedValue
```


A JSON value with the provider-owned telemetry view attached. The view is not part of the value, so the original is untouched.
Maps to a value annotated by `withTelemetryRedaction` in `@ai-sdk/provider-utils`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L13)

## Properties

<a id="Vercel_AI_ProviderUtils_TelemetryRedactedValue_Redact_"></a>
<a id="Vercel_AI_ProviderUtils_TelemetryRedactedValue_Redact"></a>

### Redact


```csharp
public Func<JsonElement, JsonElement>? Redact { get; }
```


The provider's telemetry view, or null when the value needs no redaction.

**Value:** [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L25)

<a id="Vercel_AI_ProviderUtils_TelemetryRedactedValue_Value_"></a>
<a id="Vercel_AI_ProviderUtils_TelemetryRedactedValue_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


The original value, which still carries any credentials.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L22)
