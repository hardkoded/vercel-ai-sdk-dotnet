---
title: "VertexOptions"
description: "Vertex settings. The default route stays compatible with the regional v1 publisher URL."
editUrl: false
prev: false
next: false
sidebar:
  label: "VertexOptions"
  hidden: true
---

<a id="Vercel_AI_Google_VertexOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class VertexOptions : GoogleOptions
```


Vertex settings. The default route stays compatible with the regional `v1` publisher URL.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleOptions](/api/Vercel.AI.Google/GoogleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L225)

## Constructors

<a id="Vercel_AI_Google_VertexOptions__ctor_"></a>
<a id="Vercel_AI_Google_VertexOptions__ctor"></a>

### VertexOptions()


```csharp
public VertexOptions()
```


Creates Vertex options. An empty [BaseUrl](/api/Vercel.AI.Google/GoogleOptions/#Vercel_AI_Google_GoogleOptions_BaseUrl) is replaced with the regional publisher host.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L230)

## Properties

<a id="Vercel_AI_Google_VertexOptions_DownloadFetch_"></a>
<a id="Vercel_AI_Google_VertexOptions_DownloadFetch"></a>

### DownloadFetch


```csharp
public DownloadFetch? DownloadFetch { get; set; }
```


Transport for untrusted file URLs in tool results, without provider authentication. Null uses the runtime transport.
Upstream forwards the one `fetch` option. A .NET provider authenticates through its [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) handler,
so the raw transport is a separate option.

**Value:** [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L263)

<a id="Vercel_AI_Google_VertexOptions_GenerateAuthToken_"></a>
<a id="Vercel_AI_Google_VertexOptions_GenerateAuthToken"></a>

### GenerateAuthToken


```csharp
public Func<CancellationToken, Task<string>>? GenerateAuthToken { get; set; }
```


Returns a bearer token. Called once per request when [ApiKey](/api/Vercel.AI.Google/GoogleOptions/#Vercel_AI_Google_GoogleOptions_ApiKey) is empty.
A caller-supplied Authorization header wins over the generated token.

**Value:** [Func&lt;CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L253)

<a id="Vercel_AI_Google_VertexOptions_Project_"></a>
<a id="Vercel_AI_Google_VertexOptions_Project"></a>

### Project


```csharp
public string Project { get; set; }
```


GCP project id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L238)

<a id="Vercel_AI_Google_VertexOptions_Region_"></a>
<a id="Vercel_AI_Google_VertexOptions_Region"></a>

### Region


```csharp
public string Region { get; set; }
```


Vertex location. One DNS label.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L241)

<a id="Vercel_AI_Google_VertexOptions_ToolResultDownloads_"></a>
<a id="Vercel_AI_Google_VertexOptions_ToolResultDownloads"></a>

### ToolResultDownloads


```csharp
public ToolResultDownloads? ToolResultDownloads { get; set; }
```


Settings for downloading remote files that a tool result returns. Maps to `toolResultDownloads`.

**Value:** [ToolResultDownloads](/api/Vercel.AI.Google/ToolResultDownloads/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L256)

<a id="Vercel_AI_Google_VertexOptions_UpstreamRoutes_"></a>
<a id="Vercel_AI_Google_VertexOptions_UpstreamRoutes"></a>

### UpstreamRoutes


```csharp
public bool UpstreamRoutes { get; set; }
```


When true, URLs follow the upstream host rules (`global`, `eu`, `us`, Express Mode, `v1beta1`).
The default keeps the regional `v1` publisher URL.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L247)

<a id="Vercel_AI_Google_VertexOptions_WebSocket_"></a>
<a id="Vercel_AI_Google_VertexOptions_WebSocket"></a>

### WebSocket


```csharp
public WebSocketFactory? WebSocket { get; set; }
```


Custom WebSocket implementation for Gemini Live transcription streaming. Defaults to `ClientWebSocket`. Maps to `webSocket`.

**Value:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L266)

## Methods

<a id="Vercel_AI_Google_VertexOptions_Copy_"></a>
<a id="Vercel_AI_Google_VertexOptions_Copy"></a>

### Copy()


```csharp
public VertexOptions Copy()
```


Returns a copy. The provider changes the copy, so the options the caller passed in are left as they were.

**Returns:** [VertexOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L272)
