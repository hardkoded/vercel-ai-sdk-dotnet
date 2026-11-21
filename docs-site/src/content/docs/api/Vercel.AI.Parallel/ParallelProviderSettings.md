---
title: "ParallelProviderSettings"
description: "Parallel provider settings. Maps to ParallelProviderSettings."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelProviderSettings"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelProviderSettings"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public sealed class ParallelProviderSettings
```


Parallel provider settings. Maps to `ParallelProviderSettings`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L13)

## Properties

<a id="Vercel_AI_Parallel_ParallelProviderSettings_ApiKey_"></a>
<a id="Vercel_AI_Parallel_ParallelProviderSettings_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


API key. Defaults to the `PARALLEL_API_KEY` environment variable, read when a request is made.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L16)

<a id="Vercel_AI_Parallel_ParallelProviderSettings_BaseUrl_"></a>
<a id="Vercel_AI_Parallel_ParallelProviderSettings_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


API base URL. Defaults to `https://api.parallel.ai/v1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L19)

<a id="Vercel_AI_Parallel_ParallelProviderSettings_Headers_"></a>
<a id="Vercel_AI_Parallel_ParallelProviderSettings_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers sent with every request.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L22)
