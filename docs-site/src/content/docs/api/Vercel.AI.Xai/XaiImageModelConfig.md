---
title: "XaiImageModelConfig"
description: "Settings of XaiImageModel. Maps to XaiImageModelConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiImageModelConfig"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiImageModelConfig"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiImageModelConfig
```


Settings of [XaiImageModel](/api/Vercel.AI.Xai/XaiImageModel/). Maps to `XaiImageModelConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L16)

## Constructors

<a id="Vercel_AI_Xai_XaiImageModelConfig__ctor_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____System_Net_Http_HttpClient_"></a>

### `XaiImageModelConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>, HttpClient)`


```csharp
public XaiImageModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers, HttpClient httpClient)
```


Creates a config. &lt;code class="paramref"&gt;headers&lt;/code&gt; maps `headers`: the model adds no headers of its own.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L19)

## Properties

<a id="Vercel_AI_Xai_XaiImageModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


API base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L36)

<a id="Vercel_AI_Xai_XaiImageModelConfig_CurrentDate_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig_CurrentDate"></a>

### CurrentDate


```csharp
public Func<DateTime>? CurrentDate { get; set; }
```


Sets the response timestamp and defaults to UTC now. Maps to `_internal.currentDate`.

**Value:** [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L48)

<a id="Vercel_AI_Xai_XaiImageModelConfig_DownloadHttpClient_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig_DownloadHttpClient"></a>

### DownloadHttpClient


```csharp
public HttpClient? DownloadHttpClient { get; set; }
```


HTTP client that downloads URL-only images. Defaults to a client that does not follow redirects.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L45)

<a id="Vercel_AI_Xai_XaiImageModelConfig_Headers_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig_Headers"></a>

### Headers


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }
```


Resolves the request headers, including the credential.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L39)

<a id="Vercel_AI_Xai_XaiImageModelConfig_HttpClient_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


Maps `fetch`: the HTTP client for API calls.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L42)

<a id="Vercel_AI_Xai_XaiImageModelConfig_Provider_"></a>
<a id="Vercel_AI_Xai_XaiImageModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, for example `xai.image`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L33)
