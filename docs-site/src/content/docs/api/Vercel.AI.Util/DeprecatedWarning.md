---
title: "DeprecatedWarning"
description: "A deprecated setting."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeprecatedWarning"
  hidden: true
---

<a id="Vercel_AI_Util_DeprecatedWarning"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DeprecatedWarning : ModelWarning
```


A deprecated setting.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelWarning](/api/Vercel.AI.Util/ModelWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L63)

## Constructors

<a id="Vercel_AI_Util_DeprecatedWarning__ctor_"></a>
<a id="Vercel_AI_Util_DeprecatedWarning__ctor_System_String_System_String_"></a>

### DeprecatedWarning(string, string)


```csharp
public DeprecatedWarning(string setting, string message)
```


Creates the warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `setting` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L66)

## Properties

<a id="Vercel_AI_Util_DeprecatedWarning_Message_"></a>
<a id="Vercel_AI_Util_DeprecatedWarning_Message"></a>

### Message


```csharp
public string Message { get; }
```


Replacement guidance.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L82)

<a id="Vercel_AI_Util_DeprecatedWarning_Setting_"></a>
<a id="Vercel_AI_Util_DeprecatedWarning_Setting"></a>

### Setting


```csharp
public string Setting { get; }
```


Deprecated setting name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L79)

<a id="Vercel_AI_Util_DeprecatedWarning_Type_"></a>
<a id="Vercel_AI_Util_DeprecatedWarning_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Warning kind: `unsupported`, `compatibility`, `deprecated`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L73)
