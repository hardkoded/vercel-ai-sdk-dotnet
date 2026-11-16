---
title: "TakoSearchOutput"
description: "Output of the Tako search tool: a TakoSearchResponse or a GatewayToolError. Maps to TakoSearchOutput (TakoSearchError | TakoSearchResponse, where TakoSearchE..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoSearchOutput"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoSearchOutput"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
[JsonConverter(typeof(TakoSearchOutputConverter))]
public sealed class TakoSearchOutput
```


Output of the Tako search tool: a [TakoSearchResponse](/api/Vercel.AI.Gateway/TakoSearchResponse/) or a [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/). Maps to
`TakoSearchOutput` (`TakoSearchError | TakoSearchResponse`, where `TakoSearchError` is `GatewayToolError`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1154)

## Properties

<a id="Vercel_AI_Gateway_TakoSearchOutput_Error_"></a>
<a id="Vercel_AI_Gateway_TakoSearchOutput_Error"></a>

### Error


```csharp
public GatewayToolError? Error { get; set; }
```


The error (`TakoSearchError`). Null when the tool returned a response.

**Value:** [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1161)

<a id="Vercel_AI_Gateway_TakoSearchOutput_Response_"></a>
<a id="Vercel_AI_Gateway_TakoSearchOutput_Response"></a>

### Response


```csharp
public TakoSearchResponse? Response { get; set; }
```


The search response. Null when the tool returned an error.

**Value:** [TakoSearchResponse](/api/Vercel.AI.Gateway/TakoSearchResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1158)
