---
title: "WarningLogContext"
description: "Arguments passed to the warning logger."
editUrl: false
prev: false
next: false
sidebar:
  label: "WarningLogContext"
  hidden: true
---

<a id="Vercel_AI_Operations_WarningLogContext"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class WarningLogContext
```


Arguments passed to the warning logger.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L73)

## Constructors

<a id="Vercel_AI_Operations_WarningLogContext__ctor_"></a>
<a id="Vercel_AI_Operations_WarningLogContext__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_String_System_String_"></a>

### `WarningLogContext(IReadOnlyList<OperationWarning>, string?, string?)`


```csharp
public WarningLogContext(IReadOnlyList<OperationWarning> warnings, string? provider, string? model)
```


Creates a log context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L76)

## Properties

<a id="Vercel_AI_Operations_WarningLogContext_Model_"></a>
<a id="Vercel_AI_Operations_WarningLogContext_Model"></a>

### Model


```csharp
public string? Model { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L90)

<a id="Vercel_AI_Operations_WarningLogContext_Provider_"></a>
<a id="Vercel_AI_Operations_WarningLogContext_Provider"></a>

### Provider


```csharp
public string? Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L87)

<a id="Vercel_AI_Operations_WarningLogContext_Warnings_"></a>
<a id="Vercel_AI_Operations_WarningLogContext_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from the call.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L84)
