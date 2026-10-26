---
title: "KlingAIAuth"
description: "Resolves the bearer token for KlingAI API requests. KlingAI accepts a single API key sent as a bearer token, or a legacy access key / secret key pair that si..."
editUrl: false
prev: false
next: false
sidebar:
  label: "KlingAIAuth"
  hidden: true
---

<a id="Vercel_AI_KlingAI_KlingAIAuth"></a>

Namespace: [Vercel.AI.KlingAI](/api/Vercel.AI.KlingAI/)


```csharp
public static class KlingAIAuth
```


Resolves the bearer token for KlingAI API requests.
KlingAI accepts a single API key sent as a bearer token, or a legacy access key / secret key pair that signs a short-lived JWT.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIAuth.cs#L16)

## Fields

<a id="Vercel_AI_KlingAI_KlingAIAuth_AccessKeyEnvironmentVariable"></a>

### AccessKeyEnvironmentVariable


```csharp
public const string AccessKeyEnvironmentVariable = "KLINGAI_ACCESS_KEY"
```


Environment variable that holds the legacy access key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIAuth.cs#L22)

<a id="Vercel_AI_KlingAI_KlingAIAuth_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public const string ApiKeyEnvironmentVariable = "KLINGAI_API_KEY"
```


Environment variable that holds the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIAuth.cs#L19)

<a id="Vercel_AI_KlingAI_KlingAIAuth_SecretKeyEnvironmentVariable"></a>

### SecretKeyEnvironmentVariable


```csharp
public const string SecretKeyEnvironmentVariable = "KLINGAI_SECRET_KEY"
```


Environment variable that holds the legacy secret key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIAuth.cs#L25)

## Methods

<a id="Vercel_AI_KlingAI_KlingAIAuth_GenerateAuthToken_"></a>
<a id="Vercel_AI_KlingAI_KlingAIAuth_GenerateAuthToken_System_String_System_String_"></a>

### GenerateAuthToken(string?, string?)


```csharp
public static string GenerateAuthToken(string? accessKey = null, string? secretKey = null)
```


Signs a JWT (HS256) from a legacy access key / secret key pair. The token is valid for 30 minutes.
A missing key falls back to its environment variable.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `accessKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `secretKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIAuth.cs#L68)

<a id="Vercel_AI_KlingAI_KlingAIAuth_ResolveAuthToken_"></a>
<a id="Vercel_AI_KlingAI_KlingAIAuth_ResolveAuthToken_System_String_System_String_System_String_"></a>

### ResolveAuthToken(string?, string?, string?)


```csharp
public static string ResolveAuthToken(string? apiKey = null, string? accessKey = null, string? secretKey = null)
```


Resolves the bearer token. Explicit values win over environment variables, and the API key wins over the access key / secret key pair:
`apiKey`, then `accessKey` with `secretKey`, then `KLINGAI_API_KEY`, then `KLINGAI_ACCESS_KEY` with `KLINGAI_SECRET_KEY`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `accessKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `secretKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIAuth.cs#L31)
