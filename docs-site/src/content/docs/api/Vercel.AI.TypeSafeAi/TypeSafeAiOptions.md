---
title: "TypeSafeAiOptions"
description: "TypeSafe provider settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeSafeAiOptions"
  hidden: true
---

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions"></a>

Namespace: [Vercel.AI.TypeSafeAi](/api/Vercel.AI.TypeSafeAi/)


```csharp
public sealed class TypeSafeAiOptions
```


TypeSafe provider settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L13)

## Properties

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions_ApiKey_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


API key. Falls back to `TYPESAFE_AI_API_KEY` at call time.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L16)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions_BaseUrl_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


API base URL. Defaults to `TYPESAFE_AI_BASE_URL`, then `https://api.typesafe.ai/v1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L19)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions_Headers_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiOptions_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers sent with every request.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L22)
