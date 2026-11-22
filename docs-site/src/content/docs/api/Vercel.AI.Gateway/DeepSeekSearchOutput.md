---
title: "DeepSeekSearchOutput"
description: "Output of the DeepSeek Search tool: a DeepSeekSearchResponse or a GatewayToolError. Maps to DeepSeekSearchOutput."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekSearchOutput"
  hidden: true
---

<a id="Vercel_AI_Gateway_DeepSeekSearchOutput"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
[JsonConverter(typeof(DeepSeekSearchOutputConverter))]
public sealed class DeepSeekSearchOutput
```


Output of the DeepSeek Search tool: a [DeepSeekSearchResponse](/api/Vercel.AI.Gateway/DeepSeekSearchResponse/) or a [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/). Maps to
`DeepSeekSearchOutput`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L209)

## Properties

<a id="Vercel_AI_Gateway_DeepSeekSearchOutput_Error_"></a>
<a id="Vercel_AI_Gateway_DeepSeekSearchOutput_Error"></a>

### Error


```csharp
public GatewayToolError? Error { get; set; }
```


The error. Null when the tool returned a response.

**Value:** [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L216)

<a id="Vercel_AI_Gateway_DeepSeekSearchOutput_Response_"></a>
<a id="Vercel_AI_Gateway_DeepSeekSearchOutput_Response"></a>

### Response


```csharp
public DeepSeekSearchResponse? Response { get; set; }
```


The search response. Null when the tool returned an error.

**Value:** [DeepSeekSearchResponse](/api/Vercel.AI.Gateway/DeepSeekSearchResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L213)
