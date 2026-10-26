---
title: "OpenAIWebSearchToolOptions"
description: "Options of WebSearch(OpenAIWebSearchToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIWebSearchToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIWebSearchToolOptions
```


Options of [WebSearch(OpenAIWebSearchToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_WebSearch_Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L52)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_AllowedDomains_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_AllowedDomains"></a>

### AllowedDomains


```csharp
public IReadOnlyList<string>? AllowedDomains { get; set; }
```


Domains the search may use.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L58)

<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_BlockedDomains_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_BlockedDomains"></a>

### BlockedDomains


```csharp
public IReadOnlyList<string>? BlockedDomains { get; set; }
```


Domains the search must not use.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L61)

<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_ExternalWebAccess_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_ExternalWebAccess"></a>

### ExternalWebAccess


```csharp
public bool? ExternalWebAccess { get; set; }
```


Whether the tool may reach the live web.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L55)

<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_SearchContextSize_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_SearchContextSize"></a>

### SearchContextSize


```csharp
public string? SearchContextSize { get; set; }
```


Search context size: `low`, `medium`, or `high`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L64)

<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_UserLocation_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_UserLocation"></a>

### UserLocation


```csharp
public OpenAIApproximateLocation? UserLocation { get; set; }
```


Approximate user location.

**Value:** [OpenAIApproximateLocation](/api/Vercel.AI.OpenAI/OpenAIApproximateLocation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L67)
