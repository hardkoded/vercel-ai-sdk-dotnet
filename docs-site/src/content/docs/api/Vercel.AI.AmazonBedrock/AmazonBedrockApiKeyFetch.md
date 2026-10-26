---
title: "AmazonBedrockApiKeyFetch"
description: "API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockApiKeyFetch"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockApiKeyFetch
```


API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L224)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch__ctor_System_String_"></a>

### AmazonBedrockApiKeyFetch(string)


```csharp
public AmazonBedrockApiKeyFetch(string apiKey)
```


Creates a wrapper for &lt;code class="paramref"&gt;apiKey&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L229)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch_Transport_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch_Transport"></a>

### Transport


```csharp
public Func<AmazonBedrockPreparedCall, AmazonBedrockPreparedCall>? Transport { get; set; }
```


Optional transport. When unset, the prepared call is returned directly.

**Value:** [Func&lt;AmazonBedrockPreparedCall, AmazonBedrockPreparedCall&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L235)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch_Send_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch_Send_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Send(string, string?, string?, IReadOnlyDictionary<string, string?>?)`


```csharp
public AmazonBedrockPreparedCall Send(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
```


Applies the API key and user-agent, then invokes [Transport](#Vercel_AI_AmazonBedrock_AmazonBedrockApiKeyFetch_Transport) if one is set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AmazonBedrockPreparedCall](/api/Vercel.AI.AmazonBedrock/AmazonBedrockPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L238)
