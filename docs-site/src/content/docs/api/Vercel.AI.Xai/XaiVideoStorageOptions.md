---
title: "XaiVideoStorageOptions"
description: "Store the generated video in the xAI Files API. Upstream XaiVideoStorageOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiVideoStorageOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiVideoStorageOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiVideoStorageOptions
```


Store the generated video in the xAI Files API. Upstream `XaiVideoStorageOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L21)

## Properties

<a id="Vercel_AI_Xai_XaiVideoStorageOptions_ExpiresAfter_"></a>
<a id="Vercel_AI_Xai_XaiVideoStorageOptions_ExpiresAfter"></a>

### ExpiresAfter


```csharp
public int? ExpiresAfter { get; set; }
```


Seconds until the stored file expires. Positive, at most 2,592,000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L27)

<a id="Vercel_AI_Xai_XaiVideoStorageOptions_Filename_"></a>
<a id="Vercel_AI_Xai_XaiVideoStorageOptions_Filename"></a>

### Filename


```csharp
public string Filename { get; set; }
```


File name of the stored video.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L24)

<a id="Vercel_AI_Xai_XaiVideoStorageOptions_PublicUrl_"></a>
<a id="Vercel_AI_Xai_XaiVideoStorageOptions_PublicUrl"></a>

### PublicUrl


```csharp
public bool? PublicUrl { get; set; }
```


True to create a public URL with the default lifetime. Set [PublicUrlExpiresAfter](#Vercel_AI_Xai_XaiVideoStorageOptions_PublicUrlExpiresAfter) to choose the lifetime.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L30)

<a id="Vercel_AI_Xai_XaiVideoStorageOptions_PublicUrlExpiresAfter_"></a>
<a id="Vercel_AI_Xai_XaiVideoStorageOptions_PublicUrlExpiresAfter"></a>

### PublicUrlExpiresAfter


```csharp
public int? PublicUrlExpiresAfter { get; set; }
```


Lifetime of the public URL in seconds, from 3,600 to 2,592,000. When set, a public URL is requested as an object.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L33)
