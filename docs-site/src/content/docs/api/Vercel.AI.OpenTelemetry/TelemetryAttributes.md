---
title: "TelemetryAttributes"
description: "Selects and sanitizes OpenTelemetry attribute values."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryAttributes"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public static class TelemetryAttributes
```


Selects and sanitizes OpenTelemetry attribute values.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L112)

## Methods

<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_Sanitize_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_Sanitize_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `Sanitize(IReadOnlyDictionary<string, object?>?)`


```csharp
public static IReadOnlyDictionary<string, object?> Sanitize(IReadOnlyDictionary<string, object?>? attributes)
```


Drops entries that [SanitizeValue(object)](#Vercel_AI_OpenTelemetry_TelemetryAttributes_SanitizeValue_System_Object_) rejects. A null map returns an empty map.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `attributes` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Attributes to sanitize. |

**Returns:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L175)

<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_SanitizeValue_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_SanitizeValue_System_Object_"></a>

### SanitizeValue(object?)


```csharp
public static object? SanitizeValue(object? value)
```


Returns a scalar or a homogeneous primitive array.
Non-finite numbers and mixed or empty arrays are dropped.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) | Candidate attribute value. |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L119)

<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_SelectAttributes_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_SelectAttributes_Vercel_AI_OpenTelemetry_TelemetrySelection_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `SelectAttributes(TelemetrySelection?, IReadOnlyDictionary<string, object?>?)`


```csharp
public static IReadOnlyDictionary<string, object?> SelectAttributes(TelemetrySelection? telemetry, IReadOnlyDictionary<string, object?>? attributes)
```


Selects attributes, resolving input and output callbacks and dropping values that cannot be recorded.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `telemetry` | [TelemetrySelection](/api/Vercel.AI.OpenTelemetry/TelemetrySelection/) | Recording switches. Null records every supplied value. |
| `attributes` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Literal values and [ResolvableTelemetryAttribute](/api/Vercel.AI.OpenTelemetry/ResolvableTelemetryAttribute/) callbacks. |

**Returns:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L205)

<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_SelectTelemetryAttributesAsync_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetryAttributes_SelectTelemetryAttributesAsync_Vercel_AI_OpenTelemetry_TelemetrySelection_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_Threading_CancellationToken_"></a>

### `SelectTelemetryAttributesAsync(TelemetrySelection?, IReadOnlyDictionary<string, object?>?, CancellationToken)`


```csharp
public static Task<IReadOnlyDictionary<string, object?>> SelectTelemetryAttributesAsync(TelemetrySelection? telemetry, IReadOnlyDictionary<string, object?>? attributes, CancellationToken cancellationToken = default)
```


Selects attributes without sanitizing them.
Disabled telemetry returns an empty map. Null resolver results are omitted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `telemetry` | [TelemetrySelection](/api/Vercel.AI.OpenTelemetry/TelemetrySelection/) | Recording switches. Null records every supplied value. |
| `attributes` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Literal values and [ResolvableTelemetryAttribute](/api/Vercel.AI.OpenTelemetry/ResolvableTelemetryAttribute/) callbacks. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels resolver work. |

**Returns:** [Task&lt;IReadOnlyDictionary&lt;string, object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L239)
