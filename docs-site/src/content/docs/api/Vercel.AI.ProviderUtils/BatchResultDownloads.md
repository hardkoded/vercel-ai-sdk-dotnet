---
title: "BatchResultDownloads"
description: "Settings for downloading JSON Lines batch results."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchResultDownloads"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_BatchResultDownloads"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class BatchResultDownloads
```


Settings for downloading JSON Lines batch results.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L12)

## Properties

<a id="Vercel_AI_ProviderUtils_BatchResultDownloads_MaxLineBytes_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultDownloads_MaxLineBytes"></a>

### MaxLineBytes


```csharp
public int? MaxLineBytes { get; set; }
```


Maximum UTF-8 bytes per row, excluding the LF delimiter. Defaults to 64 MiB. Must be positive.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L15)
