---
title: "AnthropicAwsApiKeyFetch"
description: "API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicAwsApiKeyFetch"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicAwsApiKeyFetch
```


API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L315)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch__ctor_System_String_"></a>

### AnthropicAwsApiKeyFetch(string)


```csharp
public AnthropicAwsApiKeyFetch(string apiKey)
```


Creates a wrapper for the static &lt;code class="paramref"&gt;apiKey&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L321)

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch__ctor_Vercel_AI_ProviderUtils_Resolvable_System_String__"></a>

### `AnthropicAwsApiKeyFetch(Resolvable<string>)`


```csharp
public AnthropicAwsApiKeyFetch(Resolvable<string> apiKey)
```


Creates a wrapper for an API key that is resolved before each request and must resolve to a non-empty key when it is a callback or a task.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | `Resolvable&lt;string&gt;` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L329)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport"></a>

### Transport


```csharp
public Func<AnthropicAwsPreparedCall, AnthropicAwsPreparedCall>? Transport { get; set; }
```


Optional transport. When unset, the prepared call is returned directly.

**Value:** [Func&lt;AnthropicAwsPreparedCall, AnthropicAwsPreparedCall&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L335)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Send_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Send_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Send(string, string?, string?, IReadOnlyDictionary<string, string?>?)`


```csharp
public AnthropicAwsPreparedCall Send(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
```


Applies the static API key and user-agent, then invokes [Transport](#Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport) if one is set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AnthropicAwsPreparedCall](/api/Vercel.AI.Anthropic/AnthropicAwsPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L367)

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_SendAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_SendAsync_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SendAsync(string, string?, string?, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<AnthropicAwsPreparedCall> SendAsync(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken = default)
```


Resolves the API key, applies it and the user-agent, then invokes [Transport](#Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport) if one is set.
A callback or task that resolves to an empty key throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AnthropicAwsPreparedCall&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L341)
