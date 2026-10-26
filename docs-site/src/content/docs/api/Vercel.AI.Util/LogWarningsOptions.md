---
title: "LogWarningsOptions"
description: "Arguments passed to Log(LogWarningsOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "LogWarningsOptions"
  hidden: true
---

<a id="Vercel_AI_Util_LogWarningsOptions"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class LogWarningsOptions
```


Arguments passed to [Log(LogWarningsOptions)](/api/Vercel.AI.Util/LogWarnings/#Vercel_AI_Util_LogWarnings_Log_Vercel_AI_Util_LogWarningsOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L105)

## Constructors

<a id="Vercel_AI_Util_LogWarningsOptions__ctor_"></a>
<a id="Vercel_AI_Util_LogWarningsOptions__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__System_String_System_String_"></a>

### `LogWarningsOptions(IReadOnlyList<ModelWarning>?, string?, string?)`


```csharp
public LogWarningsOptions(IReadOnlyList<ModelWarning>? warnings, string? provider = null, string? model = null)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `warnings` | [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L108)

## Properties

<a id="Vercel_AI_Util_LogWarningsOptions_Model_"></a>
<a id="Vercel_AI_Util_LogWarningsOptions_Model"></a>

### Model


```csharp
public string? Model { get; }
```


Model id, when the call was scoped to one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L122)

<a id="Vercel_AI_Util_LogWarningsOptions_Provider_"></a>
<a id="Vercel_AI_Util_LogWarningsOptions_Provider"></a>

### Provider


```csharp
public string? Provider { get; }
```


Provider id, when the call was scoped to one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L119)

<a id="Vercel_AI_Util_LogWarningsOptions_Warnings_"></a>
<a id="Vercel_AI_Util_LogWarningsOptions_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings returned by the provider.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L116)
