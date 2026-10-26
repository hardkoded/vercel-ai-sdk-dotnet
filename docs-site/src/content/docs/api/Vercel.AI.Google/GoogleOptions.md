---
title: "GoogleOptions"
description: "Gemini settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public class GoogleOptions
```


Gemini settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [VertexOptions](/api/Vercel.AI.Google/VertexOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L15)

## Properties

<a id="Vercel_AI_Google_GoogleOptions_ApiKey_"></a>
<a id="Vercel_AI_Google_GoogleOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L24)

<a id="Vercel_AI_Google_GoogleOptions_ApiKeyEnvironmentVariable_"></a>
<a id="Vercel_AI_Google_GoogleOptions_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public string ApiKeyEnvironmentVariable { get; set; }
```


Environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L27)

<a id="Vercel_AI_Google_GoogleOptions_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L21)

<a id="Vercel_AI_Google_GoogleOptions_BatchResultDownloads_"></a>
<a id="Vercel_AI_Google_GoogleOptions_BatchResultDownloads"></a>

### BatchResultDownloads


```csharp
public BatchResultDownloads? BatchResultDownloads { get; set; }
```


Settings for downloading JSON Lines batch results.

**Value:** [BatchResultDownloads](/api/Vercel.AI.ProviderUtils/BatchResultDownloads/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L18)

<a id="Vercel_AI_Google_GoogleOptions_EmbeddingProviderOptions_"></a>
<a id="Vercel_AI_Google_GoogleOptions_EmbeddingProviderOptions"></a>

### EmbeddingProviderOptions


```csharp
public JsonElement? EmbeddingProviderOptions { get; set; }
```


Embedding provider options read by [GoogleEmbeddingModel](/api/Vercel.AI.Google/GoogleEmbeddingModel/).

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L42)

<a id="Vercel_AI_Google_GoogleOptions_GenerateId_"></a>
<a id="Vercel_AI_Google_GoogleOptions_GenerateId"></a>

### GenerateId


```csharp
public Func<string>? GenerateId { get; set; }
```


Id factory used for tool calls that the response does not identify.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L39)

<a id="Vercel_AI_Google_GoogleOptions_Headers_"></a>
<a id="Vercel_AI_Google_GoogleOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Extra headers merged into every request.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L33)

<a id="Vercel_AI_Google_GoogleOptions_UseBearerToken_"></a>
<a id="Vercel_AI_Google_GoogleOptions_UseBearerToken"></a>

### UseBearerToken


```csharp
public bool UseBearerToken { get; set; }
```


When set, requests use `Authorization: Bearer` instead of `x-goog-api-key`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L30)

<a id="Vercel_AI_Google_GoogleOptions_UserAgent_"></a>
<a id="Vercel_AI_Google_GoogleOptions_UserAgent"></a>

### UserAgent


```csharp
public string UserAgent { get; set; }
```


User-Agent suffix. Defaults to `ai-sdk/google`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L36)
