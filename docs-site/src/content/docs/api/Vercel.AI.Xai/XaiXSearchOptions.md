---
title: "XaiXSearchOptions"
description: "Arguments of String). Maps to xSearchArgsSchema."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiXSearchOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiXSearchOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiXSearchOptions
```


Arguments of `String)`. Maps to `xSearchArgsSchema`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L40)

## Properties

<a id="Vercel_AI_Xai_XaiXSearchOptions_AllowedXHandles_"></a>
<a id="Vercel_AI_Xai_XaiXSearchOptions_AllowedXHandles"></a>

### AllowedXHandles


```csharp
public IReadOnlyList<string>? AllowedXHandles { get; init; }
```


X handles to search. At most 20.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L43)

<a id="Vercel_AI_Xai_XaiXSearchOptions_EnableImageUnderstanding_"></a>
<a id="Vercel_AI_Xai_XaiXSearchOptions_EnableImageUnderstanding"></a>

### EnableImageUnderstanding


```csharp
public bool? EnableImageUnderstanding { get; init; }
```


Whether the tool understands images in posts.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L55)

<a id="Vercel_AI_Xai_XaiXSearchOptions_EnableVideoUnderstanding_"></a>
<a id="Vercel_AI_Xai_XaiXSearchOptions_EnableVideoUnderstanding"></a>

### EnableVideoUnderstanding


```csharp
public bool? EnableVideoUnderstanding { get; init; }
```


Whether the tool understands videos in posts.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L58)

<a id="Vercel_AI_Xai_XaiXSearchOptions_ExcludedXHandles_"></a>
<a id="Vercel_AI_Xai_XaiXSearchOptions_ExcludedXHandles"></a>

### ExcludedXHandles


```csharp
public IReadOnlyList<string>? ExcludedXHandles { get; init; }
```


X handles to leave out. At most 20.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L46)

<a id="Vercel_AI_Xai_XaiXSearchOptions_FromDate_"></a>
<a id="Vercel_AI_Xai_XaiXSearchOptions_FromDate"></a>

### FromDate


```csharp
public string? FromDate { get; init; }
```


Start of the date range.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L49)

<a id="Vercel_AI_Xai_XaiXSearchOptions_ToDate_"></a>
<a id="Vercel_AI_Xai_XaiXSearchOptions_ToDate"></a>

### ToDate


```csharp
public string? ToDate { get; init; }
```


End of the date range.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L52)
