---
title: "WebFetchDefuddleOptions"
description: "Options for the HTML content extractor. Options that control the output format, logging, or network access are set by the tool. Maps to WebFetchDefuddleOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebFetchDefuddleOptions"
  hidden: true
---

<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions"></a>

Namespace: [Vercel.AI.WebFetch](/api/Vercel.AI.WebFetch/)


```csharp
public sealed class WebFetchDefuddleOptions
```


Options for the HTML content extractor. Options that control the output format, logging, or network access are set by the tool.
Maps to `WebFetchDefuddleOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchHtml.cs#L17)

## Properties

<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions_ContentSelector_"></a>
<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions_ContentSelector"></a>

### ContentSelector


```csharp
public string? ContentSelector { get; set; }
```


CSS selector of the element that holds the main content. By default the extractor picks it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchHtml.cs#L20)

<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions_RemoveHiddenElements_"></a>
<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions_RemoveHiddenElements"></a>

### RemoveHiddenElements


```csharp
public bool RemoveHiddenElements { get; set; }
```


Remove elements that are hidden from view. Defaults to `true`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchHtml.cs#L26)

<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions_RemoveImages_"></a>
<a id="Vercel_AI_WebFetch_WebFetchDefuddleOptions_RemoveImages"></a>

### RemoveImages


```csharp
public bool RemoveImages { get; set; }
```


Remove images from the result. Defaults to `false`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchHtml.cs#L23)
