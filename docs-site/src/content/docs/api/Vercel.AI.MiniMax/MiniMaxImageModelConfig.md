---
title: "MiniMaxImageModelConfig"
description: "Configuration of a MiniMaxImageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxImageModelConfig"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxImageModelConfig
```


Configuration of a [MiniMaxImageModel](/api/Vercel.AI.MiniMax/MiniMaxImageModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L16)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig__ctor_System_String_System_String_System_Func_System_Collections_Generic_Dictionary_System_String_System_String___System_Net_Http_HttpClient_System_Func_System_DateTime__"></a>

### `MiniMaxImageModelConfig(string, string, Func<Dictionary<string, string>>, HttpClient, Func<DateTime>?)`


```csharp
public MiniMaxImageModelConfig(string provider, string baseUrl, Func<Dictionary<string, string>> headers, HttpClient httpClient, Func<DateTime>? currentDate = null)
```


Creates a configuration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `currentDate` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L19)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


Image API origin including the version suffix.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L32)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_CurrentDate_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_CurrentDate"></a>

### CurrentDate


```csharp
public Func<DateTime>? CurrentDate { get; }
```


Sets the response timestamp (upstream `_internal.currentDate`).

**Value:** [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L41)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_Headers_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_Headers"></a>

### Headers


```csharp
public Func<Dictionary<string, string>> Headers { get; }
```


Resolves the request headers on every call (bearer token and user agent).

**Value:** [Func&lt;Dictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L35)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_HttpClient_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


The client every request goes through (upstream `fetch`).

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L38)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_Provider_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, `minimax.image`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L29)
