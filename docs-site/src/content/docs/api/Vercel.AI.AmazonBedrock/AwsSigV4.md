---
title: "AwsSigV4"
description: "Signs an HTTP request with AWS Signature Version 4."
editUrl: false
prev: false
next: false
sidebar:
  label: "AwsSigV4"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AwsSigV4"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AwsSigV4
```


Signs an HTTP request with AWS Signature Version 4.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AwsSigV4.cs#L13)

## Methods

<a id="Vercel_AI_AmazonBedrock_AwsSigV4_Sign_"></a>
<a id="Vercel_AI_AmazonBedrock_AwsSigV4_Sign_System_Net_Http_HttpRequestMessage_System_Byte___System_String_System_String_System_String_System_String_System_String_System_DateTimeOffset_"></a>

### Sign(HttpRequestMessage, byte[], string, string, string, string, string?, DateTimeOffset)


```csharp
public static void Sign(HttpRequestMessage request, byte[] payload, string region, string service, string accessKey, string secretKey, string? sessionToken, DateTimeOffset utcNow)
```


Adds the Authorization header for &lt;code class="paramref"&gt;service&lt;/code&gt; in &lt;code class="paramref"&gt;region&lt;/code&gt;.
The headers already on &lt;code class="paramref"&gt;request&lt;/code&gt; are signed too, except values with non-ASCII characters.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [HttpRequestMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httprequestmessage) |  |
| `payload` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `region` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `service` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `accessKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `secretKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sessionToken` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `utcNow` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AwsSigV4.cs#L30)
