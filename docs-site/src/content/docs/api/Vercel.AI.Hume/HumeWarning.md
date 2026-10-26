---
title: "HumeWarning"
description: "A warning produced while building a Hume speech request."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeWarning"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeWarning"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeWarning
```


A warning produced while building a Hume speech request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L74)

## Constructors

<a id="Vercel_AI_Hume_HumeWarning__ctor_"></a>
<a id="Vercel_AI_Hume_HumeWarning__ctor_System_String_System_String_System_String_"></a>

### HumeWarning(string, string, string)


```csharp
public HumeWarning(string type, string feature, string details)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L77)

## Properties

<a id="Vercel_AI_Hume_HumeWarning_Details_"></a>
<a id="Vercel_AI_Hume_HumeWarning_Details"></a>

### Details


```csharp
public string Details { get; }
```


Explanation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L91)

<a id="Vercel_AI_Hume_HumeWarning_Feature_"></a>
<a id="Vercel_AI_Hume_HumeWarning_Feature"></a>

### Feature


```csharp
public string Feature { get; }
```


Feature that was ignored.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L88)

<a id="Vercel_AI_Hume_HumeWarning_Type_"></a>
<a id="Vercel_AI_Hume_HumeWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning type, such as `unsupported`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L85)
