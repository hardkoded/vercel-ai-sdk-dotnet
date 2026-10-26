---
title: "TelemetrySpan"
description: "Starts an Activity span and records errors on it."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetrySpan"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_TelemetrySpan"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public static class TelemetrySpan
```


Starts an Activity span and records errors on it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetrySpan.cs#L11)

## Methods

<a id="Vercel_AI_OpenTelemetry_TelemetrySpan_RecordAsync_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetrySpan_RecordAsync__1_System_String_System_Func_System_Diagnostics_Activity_System_Threading_Tasks_Task___0___System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object___System_Boolean_System_Diagnostics_ActivitySource_"></a>

### `RecordAsync<T>(string, Func<Activity?, Task<T>>, Task<IReadOnlyDictionary<string, object?>>?, bool, ActivitySource?)`


```csharp
public static Task<T> RecordAsync<T>(string name, Func<Activity?, Task<T>> action, Task<IReadOnlyDictionary<string, object?>>? attributes = null, bool endWhenDone = true, ActivitySource? activitySource = null)
```


Runs &lt;code class="paramref"&gt;action&lt;/code&gt; inside a span named &lt;code class="paramref"&gt;name&lt;/code&gt;.
An exception is recorded, the span is stopped, and the exception is rethrown.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` | Result of &lt;code class="paramref"&gt;action&lt;/code&gt;. |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Span name. |
| `action` | [Func&lt;Activity, Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Work to run. The current span is null when nothing is listening. |
| `attributes` | [Task&lt;IReadOnlyDictionary&lt;string, object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1) | Tags applied before the action runs. Null adds none. |
| `endWhenDone` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | When false, a successful span stays open. Failures still stop the span. |
| `activitySource` | [ActivitySource](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitysource) | Source for the span. Null uses [ActivitySource](/api/Vercel.AI.OpenTelemetry/OpenTelemetryAiTelemetry/#Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_ActivitySource). |

**Returns:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetrySpan.cs#L23)

<a id="Vercel_AI_OpenTelemetry_TelemetrySpan_RecordError_"></a>
<a id="Vercel_AI_OpenTelemetry_TelemetrySpan_RecordError_System_Diagnostics_Activity_System_Object_"></a>

### RecordError(Activity, object)


```csharp
public static void RecordError(Activity activity, object error)
```


Sets the span status to error.
Exceptions also record an exception event and, for [ApiException](/api/Vercel.AI.Provider/ApiException/), the HTTP status code.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `activity` | [Activity](https://learn.microsoft.com/dotnet/api/system.diagnostics.activity) | Span to update. |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) | Exception or other error value. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetrySpan.cs#L86)
