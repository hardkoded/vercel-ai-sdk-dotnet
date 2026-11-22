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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L17)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____System_Net_Http_HttpClient_System_Func_System_DateTime__"></a>

### `MiniMaxVideoModelConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>, HttpClient, Func<DateTime>?)`


```csharp
public MiniMaxVideoModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers, HttpClient httpClient, Func<DateTime>? currentDate = null)
```


Creates a configuration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `currentDate` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L20)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


API origin with no version suffix.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L39)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_CurrentDate_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_CurrentDate"></a>

### CurrentDate


```csharp
public Func<DateTime>? CurrentDate { get; }
```


Sets the response timestamp (upstream `_internal.currentDate`).

**Value:** [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L48)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Headers_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Headers"></a>

### Headers


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }
```


Resolves the request headers on every call (bearer token and user agent).

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L42)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_HttpClient_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


The client every request goes through (upstream `fetch`).

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L45)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Provider_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, `minimax.video`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModel.cs#L36)
