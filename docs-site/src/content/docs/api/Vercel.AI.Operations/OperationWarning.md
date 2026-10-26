---
title: "OperationWarning"
description: "A warning returned by a model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationWarning"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationWarning"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class OperationWarning
```


A warning returned by a model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L27)

## Constructors

<a id="Vercel_AI_Operations_OperationWarning__ctor_"></a>
<a id="Vercel_AI_Operations_OperationWarning__ctor_System_String_System_String_System_String_System_String_System_String_"></a>

### OperationWarning(string, string?, string?, string?, string?)


```csharp
public OperationWarning(string type, string? message = null, string? feature = null, string? details = null, string? setting = null)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `setting` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L30)

## Properties

<a id="Vercel_AI_Operations_OperationWarning_Details_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Details"></a>

### Details


```csharp
public string? Details { get; }
```


Extra detail for unsupported or compatibility warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L54)

<a id="Vercel_AI_Operations_OperationWarning_Feature_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Feature"></a>

### Feature


```csharp
public string? Feature { get; }
```


Feature name for unsupported or compatibility warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L51)

<a id="Vercel_AI_Operations_OperationWarning_Message_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Message"></a>

### Message


```csharp
public string? Message { get; }
```


Human-readable message for `other` warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L48)

<a id="Vercel_AI_Operations_OperationWarning_Setting_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Setting"></a>

### Setting


```csharp
public string? Setting { get; }
```


Setting name for deprecated warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L57)

<a id="Vercel_AI_Operations_OperationWarning_Type_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning type: `unsupported`, `compatibility`, `deprecated`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L45)

## Methods

<a id="Vercel_AI_Operations_OperationWarning_Other_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Other_System_String_"></a>

### Other(string)


```csharp
public static OperationWarning Other(string message)
```


Creates an `other` warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OperationWarning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L60)

<a id="Vercel_AI_Operations_OperationWarning_Unsupported_"></a>
<a id="Vercel_AI_Operations_OperationWarning_Unsupported_System_String_System_String_"></a>

### Unsupported(string, string?)


```csharp
public static OperationWarning Unsupported(string feature, string? details = null)
```


Creates an `unsupported` warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OperationWarning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L66)
