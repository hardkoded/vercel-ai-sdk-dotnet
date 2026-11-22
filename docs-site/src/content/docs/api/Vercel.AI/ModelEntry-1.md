---
title: "ModelEntry<TRequestConfig>"
description: "One provider-owned model record, which can declare several operations. Upstream ModelEntry<RequestConfig>. <code class=\"typeparamref\">TRequestConfig</code> i..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelEntry<TRequestConfig>"
  hidden: true
---

<a id="Vercel_AI_ModelEntry_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ModelEntry<TRequestConfig> where TRequestConfig : class
```


One provider-owned model record, which can declare several operations. Upstream `ModelEntry<RequestConfig>`.
&lt;code class="typeparamref"&gt;TRequestConfig&lt;/code&gt; is a provider-defined type for request encoding and defaults. Feature support
belongs in `Capabilities`, including provider-specific features.

**Type parameters**

| Name | Description |
| --- | --- |
| `TRequestConfig` | The provider-defined request configuration type. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L439)

## Properties

<a id="Vercel_AI_ModelEntry_1_Capabilities_"></a>
<a id="Vercel_AI_ModelEntry_1_Capabilities"></a>

### Capabilities


```csharp
public ModelCapabilities Capabilities { get; init; }
```


Upstream `capabilities`.

**Value:** [ModelCapabilities](/api/Vercel.AI/ModelCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L443)

<a id="Vercel_AI_ModelEntry_1_ProviderOptions_"></a>
<a id="Vercel_AI_ModelEntry_1_ProviderOptions"></a>

### ProviderOptions


```csharp
public ModelProviderOptions? ProviderOptions { get; init; }
```


Omitted sections or options are undeclared, not explicitly unsupported.

**Value:** [ModelProviderOptions](/api/Vercel.AI/ModelProviderOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L446)

<a id="Vercel_AI_ModelEntry_1_RequestConfig_"></a>
<a id="Vercel_AI_ModelEntry_1_RequestConfig"></a>

### RequestConfig


```csharp
public TRequestConfig? RequestConfig { get; init; }
```


Upstream `requestConfig`.

**Value:** `TRequestConfig`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L449)
