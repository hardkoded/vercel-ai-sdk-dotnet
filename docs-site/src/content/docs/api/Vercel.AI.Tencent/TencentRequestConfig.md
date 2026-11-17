---
title: "TencentRequestConfig"
description: "The request configuration of a Tencent catalog entry. Upstream requestConfig of tencentModelCatalog."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentRequestConfig"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentRequestConfig"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentRequestConfig
```


The request configuration of a Tencent catalog entry. Upstream `requestConfig` of `tencentModelCatalog`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L394)

## Properties

<a id="Vercel_AI_Tencent_TencentRequestConfig_Video_"></a>
<a id="Vercel_AI_Tencent_TencentRequestConfig_Video"></a>

### Video


```csharp
public TencentVideoRequestConfig? Video { get; init; }
```


Upstream `requestConfig.video`. Null for a model without a video entry.

**Value:** [TencentVideoRequestConfig](/api/Vercel.AI.Tencent/TencentVideoRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L397)
