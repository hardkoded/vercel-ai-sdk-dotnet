---
title: "KlingAIOptions"
description: "KlingAI provider settings. Adds the legacy access key and secret key to OpenAICompatibleOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "KlingAIOptions"
  hidden: true
---

<a id="Vercel_AI_KlingAI_KlingAIOptions"></a>

Namespace: [Vercel.AI.KlingAI](/api/Vercel.AI.KlingAI/)


```csharp
public sealed class KlingAIOptions : OpenAICompatibleOptions
```


KlingAI provider settings. Adds the legacy access key and secret key to [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L13)

## Properties

<a id="Vercel_AI_KlingAI_KlingAIOptions_AccessKey_"></a>
<a id="Vercel_AI_KlingAI_KlingAIOptions_AccessKey"></a>

### AccessKey


```csharp
public string? AccessKey { get; set; }
```


Legacy access key. Defaults to the `KLINGAI_ACCESS_KEY` environment variable.
Used with [SecretKey](#Vercel_AI_KlingAI_KlingAIOptions_SecretKey) to sign a short-lived JWT. Prefer [ApiKey](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKey).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L19)

<a id="Vercel_AI_KlingAI_KlingAIOptions_SecretKey_"></a>
<a id="Vercel_AI_KlingAI_KlingAIOptions_SecretKey"></a>

### SecretKey


```csharp
public string? SecretKey { get; set; }
```


Legacy secret key. Defaults to the `KLINGAI_SECRET_KEY` environment variable.
Used with [AccessKey](#Vercel_AI_KlingAI_KlingAIOptions_AccessKey) to sign a short-lived JWT. Prefer [ApiKey](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKey).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L25)
