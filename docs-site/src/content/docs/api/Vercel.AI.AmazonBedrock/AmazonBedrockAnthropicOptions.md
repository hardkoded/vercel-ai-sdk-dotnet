---
title: "AmazonBedrockAnthropicOptions"
description: "Settings for Anthropic Messages on Amazon Bedrock InvokeModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockAnthropicOptions"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockAnthropicOptions
```


Settings for Anthropic Messages on Amazon Bedrock InvokeModel.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L15)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_AccessKeyId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_AccessKeyId"></a>

### AccessKeyId


```csharp
public string? AccessKeyId { get; set; }
```


Access key. Falls back to `AWS_ACCESS_KEY_ID`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L30)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_ApiKey_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Bearer token. When set, SigV4 is not used. Falls back to `AWS_BEARER_TOKEN_BEDROCK`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L27)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_BaseUrl_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


Runtime origin. When empty, `AWS_ENDPOINT_URL_BEDROCK_RUNTIME`, then `AWS_ENDPOINT_URL`,
then the regional Bedrock Runtime host is used.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L42)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_CredentialProvider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_CredentialProvider"></a>

### CredentialProvider


```csharp
public Func<CancellationToken, Task<AmazonBedrockCredentials>>? CredentialProvider { get; set; }
```


Supplies SigV4 credentials for each request. When set, its keys are used instead of
[AccessKeyId](#Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_AccessKeyId), [SecretAccessKey](#Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_SecretAccessKey), and [SessionToken](#Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_SessionToken).
The region always comes from [Region](#Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_Region).

**Value:** [Func&lt;CancellationToken, Task&lt;AmazonBedrockCredentials&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L49)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_Headers_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; }
```


Extra headers sent on every call. The SDK user-agent suffix is appended.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L55)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_HeadersProvider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_HeadersProvider"></a>

### HeadersProvider


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? HeadersProvider { get; set; }
```


Resolves extra headers on every call, after [Headers](#Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_Headers). Its values win. A null value drops the header.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L60)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_Region_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_Region"></a>

### Region


```csharp
public string Region { get; set; }
```


AWS region. Falls back to `AWS_REGION`, then `us-east-1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L18)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_SecretAccessKey_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_SecretAccessKey"></a>

### SecretAccessKey


```csharp
public string? SecretAccessKey { get; set; }
```


Secret key. Falls back to `AWS_SECRET_ACCESS_KEY`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L33)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_SessionToken_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; set; }
```


Session token. Falls back to `AWS_SESSION_TOKEN`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L36)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_UtcNow_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_UtcNow"></a>

### UtcNow


```csharp
public Func<DateTimeOffset>? UtcNow { get; set; }
```


Clock used for signing.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L52)
