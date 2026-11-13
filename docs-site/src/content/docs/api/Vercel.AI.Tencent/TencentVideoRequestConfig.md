---
title: "TencentVideoRequestConfig"
description: "What a video model id implies for request building."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentVideoRequestConfig"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentVideoRequestConfig"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentVideoRequestConfig
```


What a video model id implies for request building.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L117)

## Properties

<a id="Vercel_AI_Tencent_TencentVideoRequestConfig_AudioAndMultiShot_"></a>
<a id="Vercel_AI_Tencent_TencentVideoRequestConfig_AudioAndMultiShot"></a>

### AudioAndMultiShot


```csharp
public bool AudioAndMultiShot { get; }
```


Whether the model accepts the audio and multi-shot settings.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L130)

<a id="Vercel_AI_Tencent_TencentVideoRequestConfig_Omni_"></a>
<a id="Vercel_AI_Tencent_TencentVideoRequestConfig_Omni"></a>

### Omni


```csharp
public bool Omni { get; }
```


Whether the model can route reference inputs to the omni endpoint.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L133)

<a id="Vercel_AI_Tencent_TencentVideoRequestConfig_Parsed_"></a>
<a id="Vercel_AI_Tencent_TencentVideoRequestConfig_Parsed"></a>

### Parsed


```csharp
public ParsedTencentVideoModelId? Parsed { get; }
```


The parsed id, or null when the id is not recognised.

**Value:** [ParsedTencentVideoModelId](/api/Vercel.AI.Tencent/ParsedTencentVideoModelId/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L127)
