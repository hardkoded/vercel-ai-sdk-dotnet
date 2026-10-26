---
title: "AmazonBedrockCredentials"
description: "AWS credentials used to sign Amazon Bedrock requests."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockCredentials"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockCredentials
```


AWS credentials used to sign Amazon Bedrock requests.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L13)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials__ctor_System_String_System_String_System_String_System_String_"></a>

### AmazonBedrockCredentials(string, string, string, string?)


```csharp
public AmazonBedrockCredentials(string region, string accessKeyId, string secretAccessKey, string? sessionToken = null)
```


Creates credentials.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `region` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `accessKeyId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `secretAccessKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sessionToken` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L16)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_AccessKeyId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_AccessKeyId"></a>

### AccessKeyId


```csharp
public string AccessKeyId { get; }
```


Access key id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L28)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_Region_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_Region"></a>

### Region


```csharp
public string Region { get; }
```


AWS region.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L25)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_SecretAccessKey_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_SecretAccessKey"></a>

### SecretAccessKey


```csharp
public string SecretAccessKey { get; }
```


Secret access key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L31)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_SessionToken_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockCredentials_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; }
```


Optional session token.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockFetch.cs#L34)
