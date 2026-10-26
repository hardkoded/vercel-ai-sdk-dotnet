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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L247)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch__ctor_System_String_"></a>

### AnthropicAwsApiKeyFetch(string)


```csharp
public AnthropicAwsApiKeyFetch(string apiKey)
```


Creates a wrapper for &lt;code class="paramref"&gt;apiKey&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L252)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport"></a>

### Transport


```csharp
public Func<AnthropicAwsPreparedCall, AnthropicAwsPreparedCall>? Transport { get; set; }
```


Optional transport. When unset, the prepared call is returned directly.

**Value:** [Func&lt;AnthropicAwsPreparedCall, AnthropicAwsPreparedCall&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L258)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Send_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Send_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Send(string, string?, string?, IReadOnlyDictionary<string, string?>?)`


```csharp
public AnthropicAwsPreparedCall Send(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
```


Applies the API key and user-agent, then invokes [Transport](#Vercel_AI_Anthropic_AnthropicAwsApiKeyFetch_Transport) if one is set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AnthropicAwsPreparedCall](/api/Vercel.AI.Anthropic/AnthropicAwsPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L261)
