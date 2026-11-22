---
title: "DeepSeekFetchResponse"
description: "Output of the DeepSeek Fetch tool. Maps to DeepSeekFetchResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekFetchResponse"
  hidden: true
---

<a id="Vercel_AI_Gateway_DeepSeekFetchResponse"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class DeepSeekFetchResponse
```


Output of the DeepSeek Fetch tool. Maps to `DeepSeekFetchResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L91)

## Properties

<a id="Vercel_AI_Gateway_DeepSeekFetchResponse_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_DeepSeekFetchResponse_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L102)

<a id="Vercel_AI_Gateway_DeepSeekFetchResponse_Content_"></a>
<a id="Vercel_AI_Gateway_DeepSeekFetchResponse_Content"></a>

### Content


```csharp
[JsonPropertyName("content")]
public string Content { get; set; }
```


Page content as Markdown.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L98)

<a id="Vercel_AI_Gateway_DeepSeekFetchResponse_Title_"></a>
<a id="Vercel_AI_Gateway_DeepSeekFetchResponse_Title"></a>

### Title


```csharp
[JsonPropertyName("title")]
public string? Title { get; set; }
```


Page title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L94)
