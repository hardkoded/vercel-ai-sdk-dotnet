---
title: "GoogleRealtimeModelConfig"
description: "Settings of GoogleRealtimeModel. Maps to GoogleRealtimeModelConfig. Headers maps the upstream Resolvable: an object, a promise, a callback or an async callba..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeModelConfig"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeModelConfig
```


Settings of [GoogleRealtimeModel](/api/Vercel.AI.Google/GoogleRealtimeModel/). Maps to `GoogleRealtimeModelConfig`.
[Headers](#Vercel_AI_Google_GoogleRealtimeModelConfig_Headers) maps the upstream `Resolvable`: an object, a promise, a callback or an async callback
all fit a function that returns a task.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L320)

## Constructors

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig__ctor_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____"></a>

### `GoogleRealtimeModelConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>)`


```csharp
public GoogleRealtimeModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers)
```


Creates a config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L323)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


API base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L338)

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Clock_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used to compute token expiry. Defaults to the system clock.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L347)

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Headers_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Headers"></a>

### Headers


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }
```


Resolves the request headers, including the credential.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L341)

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Http_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Http"></a>

### Http


```csharp
public ProviderHttp? Http { get; set; }
```


Maps `fetch`: the HTTP helper used to mint client secrets. Defaults to a shared client.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L344)

<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Provider_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, for example `google.realtime`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L335)
