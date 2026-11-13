---
title: "TencentCloudApi"
description: "Tencent Cloud API 3.0 client with TC3-HMAC-SHA256 signing. Upstream tencent-cloud-api.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentCloudApi"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentCloudApi"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public static class TencentCloudApi
```


Tencent Cloud API 3.0 client with TC3-HMAC-SHA256 signing. Upstream `tencent-cloud-api.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L36)

## Methods

<a id="Vercel_AI_Tencent_TencentCloudApi_BuildTc3CanonicalRequest_"></a>
<a id="Vercel_AI_Tencent_TencentCloudApi_BuildTc3CanonicalRequest_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `BuildTc3CanonicalRequest(string, IReadOnlyDictionary<string, string>)`


```csharp
public static (string CanonicalRequest, string SignedHeaders) BuildTc3CanonicalRequest(string payload, IReadOnlyDictionary<string, string> headers)
```


Builds the TC3 canonical request for a POST to `/` with an empty query.
Header names are lowercased and sorted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `payload` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [(string Opening, string Closing)](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L52)

<a id="Vercel_AI_Tencent_TencentCloudApi_CreateTc3Authorization_"></a>
<a id="Vercel_AI_Tencent_TencentCloudApi_CreateTc3Authorization_System_String_System_String_System_String_System_Int64_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `CreateTc3Authorization(string, string, string, long, string, IReadOnlyDictionary<string, string>)`


```csharp
public static string CreateTc3Authorization(string secretId, string secretKey, string service, long timestamp, string payload, IReadOnlyDictionary<string, string> headers)
```


Computes the `Authorization` header value for a Tencent Cloud API 3.0 request signed with TC3-HMAC-SHA256.
&lt;code class="paramref"&gt;timestamp&lt;/code&gt; is a Unix time in seconds and must match the `X-TC-Timestamp` header.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secretId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `secretKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `service` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timestamp` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `payload` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L68)

<a id="Vercel_AI_Tencent_TencentCloudApi_GetTencentCloudErrorStatusCode_"></a>
<a id="Vercel_AI_Tencent_TencentCloudApi_GetTencentCloudErrorStatusCode_System_String_"></a>

### GetTencentCloudErrorStatusCode(string)


```csharp
public static int GetTencentCloudErrorStatusCode(string code)
```


Tencent Cloud API 3.0 reports failures with HTTP 200 and a `Response.Error` code.
Maps the code family to the HTTP status it stands for so retry logic sees the real failure class.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `code` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L85)

<a id="Vercel_AI_Tencent_TencentCloudApi_Sha256Hex_"></a>
<a id="Vercel_AI_Tencent_TencentCloudApi_Sha256Hex_System_String_"></a>

### Sha256Hex(string)


```csharp
public static string Sha256Hex(string value)
```


Hex SHA-256 of &lt;code class="paramref"&gt;value&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L42)
