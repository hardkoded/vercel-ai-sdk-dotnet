---
title: "GoogleVertexAnthropicOptions"
description: "Settings for Anthropic on Vertex."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexAnthropicOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexAnthropicOptions
```


Settings for Anthropic on Vertex.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L18)

## Properties

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


Publisher base URL. When empty, it is built from the project and location.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L27)

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_GenerateAuthToken_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_GenerateAuthToken"></a>

### GenerateAuthToken


```csharp
public Func<CancellationToken, Task<string>>? GenerateAuthToken { get; set; }
```


Returns a bearer token. Called once per request. Without it, no Authorization header is added.

**Value:** [Func&lt;CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L33)

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_Headers_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Headers merged into every request. A caller-supplied Authorization value wins over the generated token.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L30)

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_Location_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_Location"></a>

### Location


```csharp
public string? Location { get; set; }
```


Location. One DNS label.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L24)

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_Project_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_Project"></a>

### Project


```csharp
public string? Project { get; set; }
```


Project id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L21)

<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_SupportsNativeStructuredOutput_"></a>
<a id="Vercel_AI_Google_GoogleVertexAnthropicOptions_SupportsNativeStructuredOutput"></a>

### SupportsNativeStructuredOutput


```csharp
public bool SupportsNativeStructuredOutput { get; set; }
```


Enables native structured output in `structuredOutputMode: "auto"` for Claude 4.5 and newer. Defaults to false.
Requires the Google Cloud organization policy to allow the `structured_outputs` feature for each model used.
This does not enable strict tool input schemas.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexPartners.cs#L40)
