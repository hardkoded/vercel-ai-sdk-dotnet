---
title: "GatewayLanguageModelEntry"
description: "A model the Gateway offers. Maps to GatewayLanguageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayLanguageModelEntry"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayLanguageModelEntry
```


A model the Gateway offers. Maps to `GatewayLanguageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L37)

## Properties

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Description_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


Gets or sets the description of the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L62)

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Id_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Id"></a>

### Id


```csharp
public string Id { get; set; }
```


Gets or sets the model id used by the remote provider in model settings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L56)

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_KnownModelTypes_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_KnownModelTypes"></a>

### KnownModelTypes


```csharp
public static IReadOnlyList<string> KnownModelTypes { get; }
```


Maps to `KNOWN_MODEL_TYPES`. Entries with another [ModelType](#Vercel_AI_Gateway_GatewayLanguageModelEntry_ModelType) are dropped.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L40)

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_ModelType_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_ModelType"></a>

### ModelType


```csharp
public string? ModelType { get; set; }
```


Gets or sets the model type, one of [KnownModelTypes](#Vercel_AI_Gateway_GatewayLanguageModelEntry_KnownModelTypes).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L71)

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Name_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


Gets or sets the display name of the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L59)

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Pricing_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Pricing"></a>

### Pricing


```csharp
public GatewayModelPricing? Pricing { get; set; }
```


Gets or sets the pricing of the model.

**Value:** [GatewayModelPricing](/api/Vercel.AI.Gateway/GatewayModelPricing/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L65)

<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Specification_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModelEntry_Specification"></a>

### Specification


```csharp
public GatewayLanguageModelSpecification Specification { get; set; }
```


Gets or sets the AI SDK specification of the model.

**Value:** [GatewayLanguageModelSpecification](/api/Vercel.AI.Gateway/GatewayLanguageModelSpecification/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L68)
