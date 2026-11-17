---
title: "MiniMaxOptions"
description: "MiniMax provider settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxOptions"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxOptions"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxOptions
```


MiniMax provider settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L13)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxOptions_ApiKey_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


MiniMax API key. Defaults to the `MINIMAX_API_KEY` environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L16)

<a id="Vercel_AI_MiniMax_MiniMaxOptions_BaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


Base URL for the API calls. Defaults to MiniMax's Anthropic-compatible endpoint (`https://api.minimax.io/anthropic/v1`).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L19)

<a id="Vercel_AI_MiniMax_MiniMaxOptions_Headers_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Headers merged into every request.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L31)

<a id="Vercel_AI_MiniMax_MiniMaxOptions_ImageBaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_ImageBaseUrl"></a>

### ImageBaseUrl


```csharp
public string? ImageBaseUrl { get; set; }
```


Image generation endpoint. Defaults to `https://api.minimax.io/v1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L28)

<a id="Vercel_AI_MiniMax_MiniMaxOptions_VideoBaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_VideoBaseUrl"></a>

### VideoBaseUrl


```csharp
public string? VideoBaseUrl { get; set; }
```


Use a different URL prefix for video generation API calls. The default prefix is `https://api.minimax.io`.
It has no version suffix and is never derived from [BaseUrl](#Vercel_AI_MiniMax_MiniMaxOptions_BaseUrl).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L25)
