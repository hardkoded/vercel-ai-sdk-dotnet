---
title: "WarningLog"
description: "Warning logger. Observer sees every call, including empty warning lists. Logger maps to globalThis.AISDKLOGWARNINGS."
editUrl: false
prev: false
next: false
sidebar:
  label: "WarningLog"
  hidden: true
---

<a id="Vercel_AI_Operations_WarningLog"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class WarningLog
```


Warning logger. [Observer](#Vercel_AI_Operations_WarningLog_Observer) sees every call, including empty warning lists.
[Logger](#Vercel_AI_Operations_WarningLog_Logger) maps to `globalThis.AI_SDK_LOG_WARNINGS`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L97)

## Properties

<a id="Vercel_AI_Operations_WarningLog_Logger_"></a>
<a id="Vercel_AI_Operations_WarningLog_Logger"></a>

### Logger


```csharp
public static object? Logger { get; set; }
```


Custom logger. `false` suppresses logging. A delegate receives nonempty warnings.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L105)

<a id="Vercel_AI_Operations_WarningLog_Observer_"></a>
<a id="Vercel_AI_Operations_WarningLog_Observer"></a>

### Observer


```csharp
public static Action<WarningLogContext>? Observer { get; set; }
```


Receives every log attempt, including an empty warning list.

**Value:** [Action&lt;WarningLogContext&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L100)

## Methods

<a id="Vercel_AI_Operations_WarningLog_Write_"></a>
<a id="Vercel_AI_Operations_WarningLog_Write_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_String_System_String_"></a>

### `Write(IReadOnlyList<OperationWarning>?, string?, string?)`


```csharp
public static void Write(IReadOnlyList<OperationWarning>? warnings, string? provider, string? model)
```


Records warnings for one model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L108)
