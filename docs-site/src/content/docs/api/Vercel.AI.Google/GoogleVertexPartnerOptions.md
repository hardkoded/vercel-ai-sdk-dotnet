---
title: "GoogleVertexPartnerOptions"
description: "Settings for the OpenAI-compatible MaaS and xAI endpoints on Vertex."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexPartnerOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexPartnerOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexPartnerOptions
```


Settings for the OpenAI-compatible MaaS and xAI endpoints on Vertex.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L35)

## Properties

<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


OpenAI-compatible base URL. When empty, it is built from the project and location.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L44)

<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_GenerateAuthToken_"></a>
<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_GenerateAuthToken"></a>

### GenerateAuthToken


```csharp
public Func<CancellationToken, Task<string>>? GenerateAuthToken { get; set; }
```


Returns a bearer token. Called once per request. The token replaces any Authorization header.

**Value:** [Func&lt;CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L50)

<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_Headers_"></a>
<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Headers set on every request. They replace request headers with the same name.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L47)

<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_Location_"></a>
<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_Location"></a>

### Location


```csharp
public string? Location { get; set; }
```


Location. One DNS label. Defaults to `global`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L41)

<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_Project_"></a>
<a id="Vercel_AI_Google_GoogleVertexPartnerOptions_Project"></a>

### Project


```csharp
public string? Project { get; set; }
```


Project id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L38)
