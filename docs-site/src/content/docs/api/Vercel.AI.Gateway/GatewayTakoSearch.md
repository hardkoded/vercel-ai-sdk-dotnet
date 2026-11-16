---
title: "GatewayTakoSearch"
description: "JSON schemas for the Gateway Tako search tool."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayTakoSearch"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayTakoSearch"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayTakoSearch
```


JSON schemas for the Gateway Tako search tool.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L10)

## Properties

<a id="Vercel_AI_Gateway_GatewayTakoSearch_InputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearch_InputSchema"></a>

### InputSchema


```csharp
public static JsonElement InputSchema { get; }
```


Input schema. Data-source descriptions name the surcharge controls.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L45)

<a id="Vercel_AI_Gateway_GatewayTakoSearch_OutputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearch_OutputSchema"></a>

### OutputSchema


```csharp
public static JsonElement OutputSchema { get; }
```


Output schema. Internal ranking fields are not part of the contract.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L48)
