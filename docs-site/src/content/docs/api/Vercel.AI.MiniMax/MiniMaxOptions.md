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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L12)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxOptions_ApiKey_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


MiniMax API key. Defaults to the `MINIMAX_API_KEY` environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L15)

<a id="Vercel_AI_MiniMax_MiniMaxOptions_BaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


Base URL for the API calls. Defaults to MiniMax's Anthropic-compatible endpoint (`https://api.minimax.io/anthropic/v1`).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L18)

<a id="Vercel_AI_MiniMax_MiniMaxOptions_Headers_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Headers merged into every request.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L21)
