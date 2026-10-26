---
title: "WebFetchToolOptions"
description: "Options of the web fetch tool. Maps to the options of createWebFetchTool."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebFetchToolOptions"
  hidden: true
---

<a id="Vercel_AI_WebFetch_WebFetchToolOptions"></a>

Namespace: [Vercel.AI.WebFetch](/api/Vercel.AI.WebFetch/)


```csharp
public sealed class WebFetchToolOptions
```


Options of the web fetch tool. Maps to the options of `createWebFetchTool`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L16)

## Properties

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_AllowedDomains_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_AllowedDomains"></a>

### AllowedDomains


```csharp
public IReadOnlyList<string>? AllowedDomains { get; set; }
```


Only allow fetching from these domains and their subdomains. Cannot be combined with [BlockedDomains](#Vercel_AI_WebFetch_WebFetchToolOptions_BlockedDomains).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L19)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_BlockedDomains_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_BlockedDomains"></a>

### BlockedDomains


```csharp
public IReadOnlyList<string>? BlockedDomains { get; set; }
```


Never fetch from these domains or their subdomains. Cannot be combined with [AllowedDomains](#Vercel_AI_WebFetch_WebFetchToolOptions_AllowedDomains).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L22)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_DefuddleOptions_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_DefuddleOptions"></a>

### DefuddleOptions


```csharp
public WebFetchDefuddleOptions? DefuddleOptions { get; set; }
```


Options for the HTML content extractor, e.g. [ContentSelector](/api/Vercel.AI.WebFetch/WebFetchDefuddleOptions/#Vercel_AI_WebFetch_WebFetchDefuddleOptions_ContentSelector) to choose the main content element.

**Value:** [WebFetchDefuddleOptions](/api/Vercel.AI.WebFetch/WebFetchDefuddleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L43)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_Fetch_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_Fetch"></a>

### Fetch


```csharp
public DownloadFetch? Fetch { get; set; }
```


Custom fetch implementation. It must validate resolved addresses, like a DNS-pinned transport would.

**Value:** [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L40)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_MaxBytes_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_MaxBytes"></a>

### MaxBytes


```csharp
public long MaxBytes { get; set; }
```


Maximum response size in bytes. Defaults to 10 MiB.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L34)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_MaxLength_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_MaxLength"></a>

### MaxLength


```csharp
public int MaxLength { get; set; }
```


Maximum characters returned per call. Longer content can be read with the `offset` input. Defaults to 50,000.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L31)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_RequireUrlInUserMessage_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_RequireUrlInUserMessage"></a>

### RequireUrlInUserMessage


```csharp
public bool RequireUrlInUserMessage { get; set; }
```


Only fetch URLs that appear in user messages. Prevents the model from fetching URLs it constructed itself,
e.g. to exfiltrate data. Defaults to `false`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L28)

<a id="Vercel_AI_WebFetch_WebFetchToolOptions_Timeout_"></a>
<a id="Vercel_AI_WebFetch_WebFetchToolOptions_Timeout"></a>

### Timeout


```csharp
public TimeSpan Timeout { get; set; }
```


Timeout for fetching each URL, including redirects. Defaults to 30 seconds.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L37)
