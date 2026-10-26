---
title: "VoyageWarning"
description: "A compatibility warning from a Voyage rerank call."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageWarning"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageWarning"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageWarning
```


A compatibility warning from a Voyage rerank call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L68)

## Constructors

<a id="Vercel_AI_Voyage_VoyageWarning__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageWarning__ctor_System_String_System_String_System_String_"></a>

### VoyageWarning(string, string, string)


```csharp
public VoyageWarning(string type, string feature, string details)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L71)

## Properties

<a id="Vercel_AI_Voyage_VoyageWarning_Details_"></a>
<a id="Vercel_AI_Voyage_VoyageWarning_Details"></a>

### Details


```csharp
public string Details { get; }
```


Explanation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L85)

<a id="Vercel_AI_Voyage_VoyageWarning_Feature_"></a>
<a id="Vercel_AI_Voyage_VoyageWarning_Feature"></a>

### Feature


```csharp
public string Feature { get; }
```


Feature name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L82)

<a id="Vercel_AI_Voyage_VoyageWarning_Type_"></a>
<a id="Vercel_AI_Voyage_VoyageWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L79)
