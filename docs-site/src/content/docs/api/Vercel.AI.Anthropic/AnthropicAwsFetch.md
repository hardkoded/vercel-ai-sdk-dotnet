---
title: "AnthropicAwsFetch"
description: "Claude Platform on AWS fetch behavior. GET and POST requests without a body skip signing. POST bodies are signed for service aws-external-anthropic."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicAwsFetch"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicAwsFetch"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicAwsFetch
```


Claude Platform on AWS fetch behavior. GET and POST requests without a body skip signing.
POST bodies are signed for service `aws-external-anthropic`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L71)

## Fields

<a id="Vercel_AI_Anthropic_AnthropicAwsFetch_Service"></a>

### Service


```csharp
public const string Service = "aws-external-anthropic"
```


SigV4 service name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L77)

<a id="Vercel_AI_Anthropic_AnthropicAwsFetch_UserAgent"></a>

### UserAgent


```csharp
public static readonly string UserAgent
```


User agent sent on every AWS request. The runtime segment is dotnet.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L74)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicAwsFetch_Prepare_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsFetch_Prepare_System_String_System_String_System_String_System_Byte___System_Text_Json_Nodes_JsonNode_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__Vercel_AI_Anthropic_AnthropicAwsCredentials_System_Nullable_System_DateTimeOffset__"></a>

### `Prepare(string, string?, string?, byte[]?, JsonNode?, IReadOnlyDictionary<string, string?>?, IReadOnlyDictionary<string, string?>?, AnthropicAwsCredentials, DateTimeOffset?)`


```csharp
public static AnthropicAwsPreparedCall Prepare(string url, string? method, string? body, byte[]? binaryBody, JsonNode? objectBody, IReadOnlyDictionary<string, string?>? headers, IReadOnlyDictionary<string, string?>? requestHeaders, AnthropicAwsCredentials credentials, DateTimeOffset? utcNow = null)
```


Prepares a call. Non-POST requests and POST requests without a body are not signed.
String bodies are left unchanged. Byte bodies are decoded as UTF-8. Other objects are JSON.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `binaryBody` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `objectBody` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `requestHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `credentials` | [AnthropicAwsCredentials](/api/Vercel.AI.Anthropic/AnthropicAwsCredentials/) |  |
| `utcNow` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |

**Returns:** [AnthropicAwsPreparedCall](/api/Vercel.AI.Anthropic/AnthropicAwsPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L83)

<a id="Vercel_AI_Anthropic_AnthropicAwsFetch_PrepareApiKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsFetch_PrepareApiKey_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `PrepareApiKey(string, string, string?, string?, IReadOnlyDictionary<string, string?>?)`


```csharp
public static AnthropicAwsPreparedCall PrepareApiKey(string apiKey, string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
```


Adds `x-api-key` and the AWS user-agent without signing.
The API key replaces any caller-supplied `x-api-key`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AnthropicAwsPreparedCall](/api/Vercel.AI.Anthropic/AnthropicAwsPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L120)
