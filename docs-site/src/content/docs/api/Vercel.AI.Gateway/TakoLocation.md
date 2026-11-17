---
title: "TakoLocation"
description: "End-user coordinates for localized results. Maps to TakoSearchConfig['location']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoLocation"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoLocation"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoLocation
```


End-user coordinates for localized results. Maps to `TakoSearchConfig['location']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L589)

## Properties

<a id="Vercel_AI_Gateway_TakoLocation_Latitude_"></a>
<a id="Vercel_AI_Gateway_TakoLocation_Latitude"></a>

### Latitude


```csharp
public double Latitude { get; set; }
```


Latitude between -90 and 90.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L592)

<a id="Vercel_AI_Gateway_TakoLocation_Longitude_"></a>
<a id="Vercel_AI_Gateway_TakoLocation_Longitude"></a>

### Longitude


```csharp
public double Longitude { get; set; }
```


Longitude between -180 and 180.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L595)
