---
title: "TencentVideoModelEntry"
description: "The catalog entry of a video model. Upstream getTencentVideoModelEntry result."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentVideoModelEntry"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentVideoModelEntry"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentVideoModelEntry
```


The catalog entry of a video model. Upstream `getTencentVideoModelEntry` result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L304)

## Properties

<a id="Vercel_AI_Tencent_TencentVideoModelEntry_Capabilities_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelEntry_Capabilities"></a>

### Capabilities


```csharp
public TencentVideoCapabilities Capabilities { get; }
```


Upstream `capabilities.video`.

**Value:** [TencentVideoCapabilities](/api/Vercel.AI.Tencent/TencentVideoCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L314)

<a id="Vercel_AI_Tencent_TencentVideoModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }
```


Support per video option in [Video](/api/Vercel.AI.Tencent/TencentProviderOptions/#Vercel_AI_Tencent_TencentProviderOptions_Video).

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L317)

<a id="Vercel_AI_Tencent_TencentVideoModelEntry_RequestConfig_"></a>
<a id="Vercel_AI_Tencent_TencentVideoModelEntry_RequestConfig"></a>

### RequestConfig


```csharp
public TencentVideoRequestConfig RequestConfig { get; }
```


Upstream `requestConfig.video`.

**Value:** [TencentVideoRequestConfig](/api/Vercel.AI.Tencent/TencentVideoRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L320)
