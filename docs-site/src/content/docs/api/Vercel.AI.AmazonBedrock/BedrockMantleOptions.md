---
title: "BedrockMantleOptions"
description: "Settings for the OpenAI-compatible Bedrock Mantle endpoint."
editUrl: false
prev: false
next: false
sidebar:
  label: "BedrockMantleOptions"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class BedrockMantleOptions
```


Settings for the OpenAI-compatible Bedrock Mantle endpoint.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L14)

## Properties

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_AccessKeyId_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_AccessKeyId"></a>

### AccessKeyId


```csharp
public string? AccessKeyId { get; set; }
```


Access key. Falls back to `AWS_ACCESS_KEY_ID`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L29)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_ApiKey_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Bearer token. When set, SigV4 is not used. Falls back to `AWS_BEARER_TOKEN_BEDROCK`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L26)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_BaseUrl_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


Origin including the version path. When empty, the regional Mantle host is used.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L38)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_CredentialProvider_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_CredentialProvider"></a>

### CredentialProvider


```csharp
public Func<CancellationToken, Task<AmazonBedrockCredentials>>? CredentialProvider { get; set; }
```


Supplies SigV4 credentials for each request. When set, its keys are used instead of
[AccessKeyId](#Vercel_AI_AmazonBedrock_BedrockMantleOptions_AccessKeyId), [SecretAccessKey](#Vercel_AI_AmazonBedrock_BedrockMantleOptions_SecretAccessKey), and [SessionToken](#Vercel_AI_AmazonBedrock_BedrockMantleOptions_SessionToken).
The region always comes from [Region](#Vercel_AI_AmazonBedrock_BedrockMantleOptions_Region).

**Value:** [Func&lt;CancellationToken, Task&lt;AmazonBedrockCredentials&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L45)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_Headers_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string> Headers { get; }
```


Extra headers sent on every call. The SDK user-agent suffix is appended.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L51)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_HeadersProvider_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_HeadersProvider"></a>

### HeadersProvider


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? HeadersProvider { get; set; }
```


Resolves extra headers on every call, after [Headers](#Vercel_AI_AmazonBedrock_BedrockMantleOptions_Headers). Its values win. A null value drops the header.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L56)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_Region_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_Region"></a>

### Region


```csharp
public string Region { get; set; }
```


AWS region. Falls back to `AWS_REGION`, then `us-east-1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L17)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_SecretAccessKey_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_SecretAccessKey"></a>

### SecretAccessKey


```csharp
public string? SecretAccessKey { get; set; }
```


Secret key. Falls back to `AWS_SECRET_ACCESS_KEY`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L32)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_SessionToken_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; set; }
```


Session token. Falls back to `AWS_SESSION_TOKEN`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L35)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_UtcNow_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleOptions_UtcNow"></a>

### UtcNow


```csharp
public Func<DateTimeOffset>? UtcNow { get; set; }
```


Clock used for signing.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L48)
