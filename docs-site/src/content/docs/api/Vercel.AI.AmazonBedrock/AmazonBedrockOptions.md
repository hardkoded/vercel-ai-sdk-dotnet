---
title: "AmazonBedrockOptions"
description: "Amazon Bedrock settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockOptions"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockOptions
```


Amazon Bedrock settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L19)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_AccessKeyId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_AccessKeyId"></a>

### AccessKeyId


```csharp
public string? AccessKeyId { get; set; }
```


Access key. Falls back to `AWS_ACCESS_KEY_ID`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L43)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_AgentRuntimeBaseUrl_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_AgentRuntimeBaseUrl"></a>

### AgentRuntimeBaseUrl


```csharp
public string? AgentRuntimeBaseUrl { get; set; }
```


Agent Runtime origin for rerank. Falls back to `AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME`, then
`AWS_ENDPOINT_URL`. When none is set, the host is `https://bedrock-agent-runtime.{region}.{suffix}`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L40)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_BaseUrl_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


Bedrock Runtime origin. Falls back to `AWS_ENDPOINT_URL_BEDROCK_RUNTIME`, then `AWS_ENDPOINT_URL`.
When none is set, the host is `https://bedrock-runtime.{region}.{suffix}`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L34)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_Headers_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string?> Headers { get; set; }
```


Headers sent with every request. Call headers override them.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L57)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_HeadersProvider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_HeadersProvider"></a>

### HeadersProvider


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>? HeadersProvider { get; set; }
```


Resolves extra headers on every call, like upstream's resolvable `headers`. Its values win over
[Headers](#Vercel_AI_AmazonBedrock_AmazonBedrockOptions_Headers), and a null value drops a header. Call headers still win over both.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L63)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_Region_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_Region"></a>

### Region


```csharp
public string Region { get; set; }
```


AWS region. Falls back to `AWS_REGION`, then `us-east-1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L22)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_SecretAccessKey_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_SecretAccessKey"></a>

### SecretAccessKey


```csharp
public string? SecretAccessKey { get; set; }
```


Secret key. Falls back to `AWS_SECRET_ACCESS_KEY`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L46)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_SessionToken_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; set; }
```


Session token. Falls back to `AWS_SESSION_TOKEN`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L49)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_UtcNow_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockOptions_UtcNow"></a>

### UtcNow


```csharp
public Func<DateTimeOffset>? UtcNow { get; set; }
```


Clock used for signing. Tests can pin this.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L52)
