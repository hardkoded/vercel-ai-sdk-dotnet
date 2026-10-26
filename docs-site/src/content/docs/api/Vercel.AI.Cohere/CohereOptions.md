---
title: "CohereOptions"
description: "Cohere settings. The default origin is https://api.cohere.com/v2."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereOptions"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereOptions"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereOptions
```


Cohere settings. The default origin is `https://api.cohere.com/v2`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L14)

## Properties

<a id="Vercel_AI_Cohere_CohereOptions_ApiKey_"></a>
<a id="Vercel_AI_Cohere_CohereOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L20)

<a id="Vercel_AI_Cohere_CohereOptions_BaseUrl_"></a>
<a id="Vercel_AI_Cohere_CohereOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L17)

<a id="Vercel_AI_Cohere_CohereOptions_Headers_"></a>
<a id="Vercel_AI_Cohere_CohereOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Headers sent with every request. Call headers override them.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L23)
