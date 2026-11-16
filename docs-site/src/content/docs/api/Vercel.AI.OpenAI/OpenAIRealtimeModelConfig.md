---
title: "OpenAIRealtimeModelConfig"
description: "Settings of OpenAIRealtimeModel. Maps to OpenAIRealtimeModelConfig. Headers maps the upstream Resolvable: an object, a promise, a callback or an async callba..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModelConfig"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModelConfig
```


Settings of [OpenAIRealtimeModel](/api/Vercel.AI.OpenAI/OpenAIRealtimeModel/). Maps to `OpenAIRealtimeModelConfig`.
[Headers](#Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_Headers) maps the upstream `Resolvable`: an object, a promise, a callback or an async callback
all fit a function that returns a task.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L37)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____"></a>

### `OpenAIRealtimeModelConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>)`


```csharp
public OpenAIRealtimeModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers)
```


Creates a config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L40)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


API base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L51)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_Headers"></a>

### Headers


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }
```


Resolves the request headers, including the credential.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L54)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_HttpClient_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_HttpClient"></a>

### HttpClient


```csharp
public HttpClient? HttpClient { get; set; }
```


Maps `fetch`: the HTTP client used to mint client secrets. Defaults to a shared client.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L57)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, for example `openai.realtime`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L48)
