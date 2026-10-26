---
title: "AmazonBedrockFetch"
description: "Amazon Bedrock fetch behavior. GET requests and POST requests without a body skip signing. POST bodies are signed with SigV4. Caller headers are signed, exce..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockFetch"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockFetch
```


Amazon Bedrock fetch behavior. GET requests and POST requests without a body skip signing.
POST bodies are signed with SigV4. Caller headers are signed, except values with non-ASCII characters.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L70)

## Fields

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch_DefaultService"></a>

### DefaultService


```csharp
public const string DefaultService = "bedrock"
```


SigV4 service name used when none is given.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L73)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch_UserAgent"></a>

### UserAgent


```csharp
public static readonly string UserAgent
```


User agent sent on every request. The runtime segment is dotnet.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L76)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch_Prepare_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch_Prepare_System_String_System_String_System_String_System_Byte___System_Text_Json_Nodes_JsonNode_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_System_String_System_Nullable_System_DateTimeOffset__"></a>

### `Prepare(string, string?, string?, byte[]?, JsonNode?, IReadOnlyDictionary<string, string?>?, IReadOnlyDictionary<string, string?>?, AmazonBedrockCredentials, string, DateTimeOffset?)`


```csharp
public static AmazonBedrockPreparedCall Prepare(string url, string? method, string? body, byte[]? binaryBody, JsonNode? objectBody, IReadOnlyDictionary<string, string?>? headers, IReadOnlyDictionary<string, string?>? requestHeaders, AmazonBedrockCredentials credentials, string service = "bedrock", DateTimeOffset? utcNow = null)
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
| `credentials` | [AmazonBedrockCredentials](/api/Vercel.AI.AmazonBedrock/AmazonBedrockCredentials/) |  |
| `service` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `utcNow` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |

**Returns:** [AmazonBedrockPreparedCall](/api/Vercel.AI.AmazonBedrock/AmazonBedrockPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L82)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch_PrepareApiKey_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockFetch_PrepareApiKey_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `PrepareApiKey(string, string, string?, string?, IReadOnlyDictionary<string, string?>?)`


```csharp
public static AmazonBedrockPreparedCall PrepareApiKey(string apiKey, string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
```


Adds `Authorization: Bearer` and the user-agent without signing.
The API key replaces any caller-supplied `Authorization`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AmazonBedrockPreparedCall](/api/Vercel.AI.AmazonBedrock/AmazonBedrockPreparedCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L120)
