---
title: "CompatibilityWarning"
description: "A feature running in compatibility mode."
editUrl: false
prev: false
next: false
sidebar:
  label: "CompatibilityWarning"
  hidden: true
---

<a id="Vercel_AI_Util_CompatibilityWarning"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class CompatibilityWarning : ModelWarning
```


A feature running in compatibility mode.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelWarning](/api/Vercel.AI.Util/ModelWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L40)

## Constructors

<a id="Vercel_AI_Util_CompatibilityWarning__ctor_"></a>
<a id="Vercel_AI_Util_CompatibilityWarning__ctor_System_String_System_String_"></a>

### CompatibilityWarning(string, string?)


```csharp
public CompatibilityWarning(string feature, string? details = null)
```


Creates the warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L43)

## Properties

<a id="Vercel_AI_Util_CompatibilityWarning_Details_"></a>
<a id="Vercel_AI_Util_CompatibilityWarning_Details"></a>

### Details


```csharp
public string? Details { get; }
```


Optional extra detail.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L59)

<a id="Vercel_AI_Util_CompatibilityWarning_Feature_"></a>
<a id="Vercel_AI_Util_CompatibilityWarning_Feature"></a>

### Feature


```csharp
public string Feature { get; }
```


Feature name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L56)

<a id="Vercel_AI_Util_CompatibilityWarning_Type_"></a>
<a id="Vercel_AI_Util_CompatibilityWarning_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Warning kind: `unsupported`, `compatibility`, `deprecated`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L50)
