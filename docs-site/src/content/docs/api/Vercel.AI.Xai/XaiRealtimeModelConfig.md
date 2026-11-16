---
title: "XaiRealtimeModelConfig"
description: "Settings of XaiRealtimeModel. Maps to XaiRealtimeModelConfig. Headers maps the upstream Resolvable: an object, a promise, a callback or an async callback all..."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiRealtimeModelConfig"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiRealtimeModelConfig"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiRealtimeModelConfig
```


Settings of [XaiRealtimeModel](/api/Vercel.AI.Xai/XaiRealtimeModel/). Maps to `XaiRealtimeModelConfig`.
[Headers](#Vercel_AI_Xai_XaiRealtimeModelConfig_Headers) maps the upstream `Resolvable`: an object, a promise, a callback or an async callback
all fit a function that returns a task.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L38)

## Constructors

<a id="Vercel_AI_Xai_XaiRealtimeModelConfig__ctor_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModelConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____"></a>

### `XaiRealtimeModelConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>)`


```csharp
public XaiRealtimeModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers)
```


Creates a config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L41)

## Properties

<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


API base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L52)

<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_Headers_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_Headers"></a>

### Headers


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }
```


Resolves the request headers, including the credential.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L55)

<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_HttpClient_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient? HttpClient { get; set; }
```


Maps `fetch`: the HTTP client used to mint client secrets. Defaults to a shared client.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L58)

<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_Provider_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, for example `xai.realtime`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L49)
