---
title: "ScaleDownProviderSettings"
description: "ScaleDown provider settings. Maps to ScaleDownProviderSettings."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownProviderSettings"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public sealed class ScaleDownProviderSettings
```


ScaleDown provider settings. Maps to `ScaleDownProviderSettings`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L13)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings_ApiKey_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


API key. Defaults to the `SCALEDOWN_API_KEY` environment variable, read when a request is made.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L16)

<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings_BaseUrl_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


API base URL. Defaults to `https://api.scaledown.xyz`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L19)

<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings_Headers_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProviderSettings_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers sent with every request.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L22)
