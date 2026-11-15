---
title: "MiniMaxVideoModelConfig"
description: "Configuration of a MiniMaxVideoModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxVideoModelConfig"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxVideoModelConfig
```


Configuration of a [MiniMaxVideoModel](/api/Vercel.AI.MiniMax/MiniMaxVideoModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L11)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig__ctor_System_String_System_String_System_Func_System_Collections_Generic_Dictionary_System_String_System_String___System_Net_Http_HttpClient_"></a>

### `MiniMaxVideoModelConfig(string, string, Func<Dictionary<string, string>>, HttpClient)`


```csharp
public MiniMaxVideoModelConfig(string provider, string baseUrl, Func<Dictionary<string, string>> headers, HttpClient httpClient)
```


Creates a configuration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L14)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


API origin with no version suffix.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L26)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Headers_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Headers"></a>

### Headers


```csharp
public Func<Dictionary<string, string>> Headers { get; }
```


Resolves the request headers on every call (bearer token and user agent).

**Value:** [Func&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L29)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_HttpClient_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


The client every request goes through (upstream `fetch`).

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L32)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Provider_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, `minimax.video`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L23)
