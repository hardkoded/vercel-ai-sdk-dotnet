---
title: "ToolResultDownloads"
description: "Settings for downloading remote files that a tool result returns. Maps to toolResultDownloads."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolResultDownloads"
  hidden: true
---

<a id="Vercel_AI_Google_ToolResultDownloads"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class ToolResultDownloads
```


Settings for downloading remote files that a tool result returns. Maps to `toolResultDownloads`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/DownloadToolResultFiles.cs#L13)

## Properties

<a id="Vercel_AI_Google_ToolResultDownloads_MaxBytes_"></a>
<a id="Vercel_AI_Google_ToolResultDownloads_MaxBytes"></a>

### MaxBytes


```csharp
public long? MaxBytes { get; set; }
```


Maximum size of each downloaded file in bytes. Defaults to 7 MiB.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/DownloadToolResultFiles.cs#L16)
