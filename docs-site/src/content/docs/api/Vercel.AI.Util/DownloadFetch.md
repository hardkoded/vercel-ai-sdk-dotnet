---
title: "DownloadFetch"
description: "Fetches one URL for Download."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadFetch"
  hidden: true
---

<a id="Vercel_AI_Util_DownloadFetch"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public delegate Task<DownloadResponse> DownloadFetch(string url, DownloadRequest request)
```


Fetches one URL for [Download](/api/Vercel.AI.Util/Download/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `request` | [DownloadRequest](/api/Vercel.AI.Util/DownloadRequest/) |  |

**Returns:** [Task&lt;DownloadResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L186)
