---
title: "DeepSeekFetchOutput"
description: "Output of the DeepSeek Fetch tool: a DeepSeekFetchResponse or a GatewayToolError. Maps to DeepSeekFetchOutput."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekFetchOutput"
  hidden: true
---

<a id="Vercel_AI_Gateway_DeepSeekFetchOutput"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
[JsonConverter(typeof(DeepSeekFetchOutputConverter))]
public sealed class DeepSeekFetchOutput
```


Output of the DeepSeek Fetch tool: a [DeepSeekFetchResponse](/api/Vercel.AI.Gateway/DeepSeekFetchResponse/) or a [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/). Maps to
`DeepSeekFetchOutput`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L110)

## Properties

<a id="Vercel_AI_Gateway_DeepSeekFetchOutput_Error_"></a>
<a id="Vercel_AI_Gateway_DeepSeekFetchOutput_Error"></a>

### Error


```csharp
public GatewayToolError? Error { get; set; }
```


The error. Null when the tool returned a response.

**Value:** [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L117)

<a id="Vercel_AI_Gateway_DeepSeekFetchOutput_Response_"></a>
<a id="Vercel_AI_Gateway_DeepSeekFetchOutput_Response"></a>

### Response


```csharp
public DeepSeekFetchResponse? Response { get; set; }
```


The fetch response. Null when the tool returned an error.

**Value:** [DeepSeekFetchResponse](/api/Vercel.AI.Gateway/DeepSeekFetchResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L114)
