---
title: "ScaleDownDecisionProviderMetadata"
description: "Provider metadata of a ScaleDown decision, under providerMetadata.scaledown."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownDecisionProviderMetadata"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public sealed class ScaleDownDecisionProviderMetadata
```


Provider metadata of a ScaleDown decision, under `providerMetadata.scaledown`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L15)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata_Confidence_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata_Confidence"></a>

### Confidence


```csharp
public IReadOnlyDictionary<string, double> Confidence { get; init; }
```


Confidence per question id. Boolean questions have none.

**Value:** [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L18)

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata_Cost_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata_Cost"></a>

### Cost


```csharp
public double? Cost { get; init; }
```


Total request cost in USD, when supplied by ScaleDown.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L24)

<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata_Legend_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownDecisionProviderMetadata_Legend"></a>

### Legend


```csharp
public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Legend { get; init; }
```


Score legend per question id.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L21)
