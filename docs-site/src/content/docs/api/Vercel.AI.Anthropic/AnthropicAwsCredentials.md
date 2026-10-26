---
title: "AnthropicAwsCredentials"
description: "AWS credentials used to sign Claude Platform requests."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicAwsCredentials"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicAwsCredentials
```


AWS credentials used to sign Claude Platform requests.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L14)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials__ctor_System_String_System_String_System_String_System_String_"></a>

### AnthropicAwsCredentials(string, string, string, string?)


```csharp
public AnthropicAwsCredentials(string region, string accessKeyId, string secretAccessKey, string? sessionToken = null)
```


Creates credentials.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `region` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `accessKeyId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `secretAccessKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sessionToken` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L17)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_AccessKeyId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_AccessKeyId"></a>

### AccessKeyId


```csharp
public string AccessKeyId { get; }
```


Access key id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L29)

<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_Region_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_Region"></a>

### Region


```csharp
public string Region { get; }
```


AWS region.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L26)

<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_SecretAccessKey_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_SecretAccessKey"></a>

### SecretAccessKey


```csharp
public string SecretAccessKey { get; }
```


Secret access key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L32)

<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_SessionToken_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsCredentials_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; }
```


Optional session token.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicAwsFetch.cs#L35)
