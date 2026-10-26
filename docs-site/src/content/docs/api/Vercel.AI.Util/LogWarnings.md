---
title: "LogWarnings"
description: "Logs provider warnings. Set Logger to false to suppress them, or to a delegate to replace the default emitter."
editUrl: false
prev: false
next: false
sidebar:
  label: "LogWarnings"
  hidden: true
---

<a id="Vercel_AI_Util_LogWarnings"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class LogWarnings
```


Logs provider warnings. Set [Logger](#Vercel_AI_Util_LogWarnings_Logger) to `false` to suppress them,
or to a delegate to replace the default emitter.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L129)

## Fields

<a id="Vercel_AI_Util_LogWarnings_FirstWarningInfoMessage"></a>

### FirstWarningInfoMessage


```csharp
public const string FirstWarningInfoMessage = "AI SDK Warning System: To turn off warning logging, set the AI_SDK_LOG_WARNINGS global to false."
```


Note emitted once before the first default warning.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L132)

## Properties

<a id="Vercel_AI_Util_LogWarnings_ConsoleWarn_"></a>
<a id="Vercel_AI_Util_LogWarnings_ConsoleWarn"></a>

### ConsoleWarn


```csharp
public static Action<string>? ConsoleWarn { get; set; }
```


`console.warn` stand-in used when [ProcessEmitWarning](#Vercel_AI_Util_LogWarnings_ProcessEmitWarning) is null.

**Value:** [Action&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L195)

<a id="Vercel_AI_Util_LogWarnings_Logger_"></a>
<a id="Vercel_AI_Util_LogWarnings_Logger"></a>

### Logger


```csharp
public static object? Logger { get; set; }
```


`false` suppresses warnings. A `Action%601` receives them.
`null` uses the default emitter.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L152)

<a id="Vercel_AI_Util_LogWarnings_ProcessEmitWarning_"></a>
<a id="Vercel_AI_Util_LogWarnings_ProcessEmitWarning"></a>

### ProcessEmitWarning


```csharp
public static Action<string, string, string?>? ProcessEmitWarning { get; set; }
```


Node `process.emitWarning` stand-in. Arguments are the message, the warning type, and the
deprecation code (`null` for ordinary warnings). `null` falls back to [ConsoleWarn](#Vercel_AI_Util_LogWarnings_ConsoleWarn).

**Value:** [Action&lt;string, string, string&gt;](https://learn.microsoft.com/dotnet/api/system.action-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L175)

## Methods

<a id="Vercel_AI_Util_LogWarnings_Log_"></a>
<a id="Vercel_AI_Util_LogWarnings_Log_Vercel_AI_Util_LogWarningsOptions_"></a>

### Log(LogWarningsOptions)


```csharp
public static void Log(LogWarningsOptions options)
```


Logs &lt;code class="paramref"&gt;options&lt;/code&gt; using the current logger settings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LogWarningsOptions](/api/Vercel.AI.Util/LogWarningsOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L225)

<a id="Vercel_AI_Util_LogWarnings_ResetState_"></a>
<a id="Vercel_AI_Util_LogWarnings_ResetState"></a>

### ResetState()


```csharp
public static void ResetState()
```


Clears the first-call note and the emitted deprecation codes so the next log emits them again.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L215)
