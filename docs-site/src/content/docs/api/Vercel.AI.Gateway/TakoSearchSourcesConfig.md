---
title: "TakoSearchSourcesConfig"
description: "Sources to search. Maps to TakoSearchConfig['sources']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoSearchSourcesConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoSearchSourcesConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoSearchSourcesConfig
```


Sources to search. Maps to `TakoSearchConfig['sources']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L471)

## Properties

<a id="Vercel_AI_Gateway_TakoSearchSourcesConfig_Data_"></a>
<a id="Vercel_AI_Gateway_TakoSearchSourcesConfig_Data"></a>

### Data


```csharp
public TakoDataSourceConfig? Data { get; set; }
```


Curated data source options. Maps to `data`.

**Value:** [TakoDataSourceConfig](/api/Vercel.AI.Gateway/TakoDataSourceConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L474)

<a id="Vercel_AI_Gateway_TakoSearchSourcesConfig_Web_"></a>
<a id="Vercel_AI_Gateway_TakoSearchSourcesConfig_Web"></a>

### Web


```csharp
public TakoWebSourceConfig? Web { get; set; }
```


Web source options. Maps to `web`.

**Value:** [TakoWebSourceConfig](/api/Vercel.AI.Gateway/TakoWebSourceConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L477)
