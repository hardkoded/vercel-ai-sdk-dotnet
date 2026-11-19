---
title: "TelemetryRedaction"
description: "Provider-owned telemetry redaction. Maps to telemetry-redaction.ts in @ai-sdk/provider-utils."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryRedaction"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TelemetryRedaction"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class TelemetryRedaction
```


Provider-owned telemetry redaction. Maps to `telemetry-redaction.ts` in `@ai-sdk/provider-utils`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L29)

## Methods

<a id="Vercel_AI_ProviderUtils_TelemetryRedaction_RedactTelemetryValue_"></a>
<a id="Vercel_AI_ProviderUtils_TelemetryRedaction_RedactTelemetryValue_System_Text_Json_JsonElement_System_Func_System_Text_Json_JsonElement_System_Text_Json_JsonElement__"></a>

### `RedactTelemetryValue(JsonElement, Func<JsonElement, JsonElement>?)`


```csharp
public static JsonElement? RedactTelemetryValue(JsonElement value, Func<JsonElement, JsonElement>? redact)
```


Applies &lt;code class="paramref"&gt;redact&lt;/code&gt; to &lt;code class="paramref"&gt;value&lt;/code&gt;. Returns null when the redactor throws, so a broken provider
redactor neither exposes credentials nor fails the call. Maps to `redactTelemetryValue`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `redact` | [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L60)

<a id="Vercel_AI_ProviderUtils_TelemetryRedaction_RedactTelemetryValue_Vercel_AI_ProviderUtils_TelemetryRedactedValue_"></a>

### RedactTelemetryValue(TelemetryRedactedValue)


```csharp
public static JsonElement? RedactTelemetryValue(TelemetryRedactedValue value)
```


Applies the provider's telemetry view, leaving an unannotated value unchanged. Maps to `redactTelemetryValue`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [TelemetryRedactedValue](/api/Vercel.AI.ProviderUtils/TelemetryRedactedValue/) |  |

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L46)

<a id="Vercel_AI_ProviderUtils_TelemetryRedaction_WithTelemetryRedaction_"></a>
<a id="Vercel_AI_ProviderUtils_TelemetryRedaction_WithTelemetryRedaction_System_Text_Json_JsonElement_System_Func_System_Text_Json_JsonElement_System_Text_Json_JsonElement__"></a>

### `WithTelemetryRedaction(JsonElement, Func<JsonElement, JsonElement>)`


```csharp
public static TelemetryRedactedValue WithTelemetryRedaction(JsonElement value, Func<JsonElement, JsonElement> redact)
```


Attaches a provider-owned telemetry view to &lt;code class="paramref"&gt;value&lt;/code&gt; without changing it. Maps to `withTelemetryRedaction`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `redact` | [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [TelemetryRedactedValue](/api/Vercel.AI.ProviderUtils/TelemetryRedactedValue/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TelemetryRedaction.cs#L32)
