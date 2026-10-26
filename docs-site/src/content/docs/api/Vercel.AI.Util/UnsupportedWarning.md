---
title: "UnsupportedWarning"
description: "A feature the provider does not support."
editUrl: false
prev: false
next: false
sidebar:
  label: "UnsupportedWarning"
  hidden: true
---

<a id="Vercel_AI_Util_UnsupportedWarning"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class UnsupportedWarning : ModelWarning
```


A feature the provider does not support.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelWarning](/api/Vercel.AI.Util/ModelWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L17)

## Constructors

<a id="Vercel_AI_Util_UnsupportedWarning__ctor_"></a>
<a id="Vercel_AI_Util_UnsupportedWarning__ctor_System_String_System_String_"></a>

### UnsupportedWarning(string, string?)


```csharp
public UnsupportedWarning(string feature, string? details = null)
```


Creates the warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L20)

## Properties

<a id="Vercel_AI_Util_UnsupportedWarning_Details_"></a>
<a id="Vercel_AI_Util_UnsupportedWarning_Details"></a>

### Details


```csharp
public string? Details { get; }
```


Optional extra detail.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L36)

<a id="Vercel_AI_Util_UnsupportedWarning_Feature_"></a>
<a id="Vercel_AI_Util_UnsupportedWarning_Feature"></a>

### Feature


```csharp
public string Feature { get; }
```


Feature name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L33)

<a id="Vercel_AI_Util_UnsupportedWarning_Type_"></a>
<a id="Vercel_AI_Util_UnsupportedWarning_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Warning kind: `unsupported`, `compatibility`, `deprecated`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L27)
