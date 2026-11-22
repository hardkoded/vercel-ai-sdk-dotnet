---
title: "DeepSeekSearchResponse"
description: "Output of the DeepSeek Search tool. Maps to DeepSeekSearchResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekSearchResponse"
  hidden: true
---

<a id="Vercel_AI_Gateway_DeepSeekSearchResponse"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class DeepSeekSearchResponse
```


Output of the DeepSeek Search tool. Maps to `DeepSeekSearchResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L194)

## Properties

<a id="Vercel_AI_Gateway_DeepSeekSearchResponse_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_DeepSeekSearchResponse_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L201)

<a id="Vercel_AI_Gateway_DeepSeekSearchResponse_Results_"></a>
<a id="Vercel_AI_Gateway_DeepSeekSearchResponse_Results"></a>

### Results


```csharp
[JsonPropertyName("results")]
public IList<DeepSeekSearchResult> Results { get; set; }
```


Search results.

**Value:** [IList&lt;DeepSeekSearchResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L197)
