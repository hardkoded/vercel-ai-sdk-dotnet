---
title: "XaiWebSearchOptions"
description: "Arguments of String). Maps to webSearchArgsSchema."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiWebSearchOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiWebSearchOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiWebSearchOptions
```


Arguments of `String)`. Maps to `webSearchArgsSchema`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L24)

## Properties

<a id="Vercel_AI_Xai_XaiWebSearchOptions_AllowedDomains_"></a>
<a id="Vercel_AI_Xai_XaiWebSearchOptions_AllowedDomains"></a>

### AllowedDomains


```csharp
public IReadOnlyList<string>? AllowedDomains { get; init; }
```


Domains to search. At most 5.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L27)

<a id="Vercel_AI_Xai_XaiWebSearchOptions_EnableImageSearch_"></a>
<a id="Vercel_AI_Xai_XaiWebSearchOptions_EnableImageSearch"></a>

### EnableImageSearch


```csharp
public bool? EnableImageSearch { get; init; }
```


Whether the tool searches for images.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L33)

<a id="Vercel_AI_Xai_XaiWebSearchOptions_EnableImageUnderstanding_"></a>
<a id="Vercel_AI_Xai_XaiWebSearchOptions_EnableImageUnderstanding"></a>

### EnableImageUnderstanding


```csharp
public bool? EnableImageUnderstanding { get; init; }
```


Whether the tool understands the images it finds.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L36)

<a id="Vercel_AI_Xai_XaiWebSearchOptions_ExcludedDomains_"></a>
<a id="Vercel_AI_Xai_XaiWebSearchOptions_ExcludedDomains"></a>

### ExcludedDomains


```csharp
public IReadOnlyList<string>? ExcludedDomains { get; init; }
```


Domains to leave out. At most 5.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L30)
