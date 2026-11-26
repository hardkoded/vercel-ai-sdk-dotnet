---
title: "StepModel"
description: "Information about the model that produced a step. Maps to the model object of StepResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "StepModel"
  hidden: true
---

<a id="Vercel_AI_StepModel"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StepModel
```


Information about the model that produced a step. Maps to the `model` object of `StepResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L353)

## Constructors

<a id="Vercel_AI_StepModel__ctor_"></a>
<a id="Vercel_AI_StepModel__ctor_System_String_System_String_"></a>

### StepModel(string, string)


```csharp
public StepModel(string provider, string modelId)
```


Creates model information.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L356)

## Properties

<a id="Vercel_AI_StepModel_ModelId_"></a>
<a id="Vercel_AI_StepModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


The ID of the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L366)

<a id="Vercel_AI_StepModel_Provider_"></a>
<a id="Vercel_AI_StepModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The provider of the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L363)
