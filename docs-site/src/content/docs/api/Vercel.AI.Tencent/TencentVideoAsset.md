---
title: "TencentVideoAsset"
description: "A VOD material registered with CreateAigcMaterial. Upstream assets entries."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentVideoAsset"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentVideoAsset"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentVideoAsset
```


A VOD material registered with `CreateAigcMaterial`. Upstream `assets` entries.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L31)

## Properties

<a id="Vercel_AI_Tencent_TencentVideoAsset_Category_"></a>
<a id="Vercel_AI_Tencent_TencentVideoAsset_Category"></a>

### Category


```csharp
public string? Category { get; set; }
```


`Image`, `Video` or `Audio`. Defaults to `Image`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L40)

<a id="Vercel_AI_Tencent_TencentVideoAsset_Url_"></a>
<a id="Vercel_AI_Tencent_TencentVideoAsset_Url"></a>

### Url


```csharp
public string Url { get; set; }
```


Asset URL, for example `asset://asset-…`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L34)

<a id="Vercel_AI_Tencent_TencentVideoAsset_Usage_"></a>
<a id="Vercel_AI_Tencent_TencentVideoAsset_Usage"></a>

### Usage


```csharp
public string? Usage { get; set; }
```


`FirstFrame`, `LastFrame` or `Reference`. Defaults to `Reference`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentVideoModelOptions.cs#L37)
