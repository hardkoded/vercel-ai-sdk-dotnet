---
title: "TencentOptions"
description: "Tencent provider settings. Adds the Tencent Cloud credentials for VOD video models to OpenAICompatibleOptions. ApiKey is the TokenHub API key, used by langua..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentOptions"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentOptions"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentOptions : OpenAICompatibleOptions
```


Tencent provider settings. Adds the Tencent Cloud credentials for VOD video models to [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/).
[ApiKey](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKey) is the TokenHub API key, used by language models and TokenHub video models.
It defaults to the `TENCENT_TOKENHUB_API_KEY` environment variable.
[BaseUrl](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_BaseUrl) is the TokenHub base URL and defaults to `https://tokenhub-intl.tencentcloudmaas.com/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L19)

## Properties

<a id="Vercel_AI_Tencent_TencentOptions_Region_"></a>
<a id="Vercel_AI_Tencent_TencentOptions_Region"></a>

### Region


```csharp
public string? Region { get; set; }
```


Tencent Cloud region sent as `X-TC-Region` on VOD requests. VOD AIGC actions do not require a region.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L34)

<a id="Vercel_AI_Tencent_TencentOptions_SecretId_"></a>
<a id="Vercel_AI_Tencent_TencentOptions_SecretId"></a>

### SecretId


```csharp
public string? SecretId { get; set; }
```


Tencent Cloud API SecretId, used by VOD video models. Defaults to the `TENCENTCLOUD_SECRET_ID` environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L22)

<a id="Vercel_AI_Tencent_TencentOptions_SecretKey_"></a>
<a id="Vercel_AI_Tencent_TencentOptions_SecretKey"></a>

### SecretKey


```csharp
public string? SecretKey { get; set; }
```


Tencent Cloud API SecretKey, used by VOD video models. Defaults to the `TENCENTCLOUD_SECRET_KEY` environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L25)

<a id="Vercel_AI_Tencent_TencentOptions_SessionToken_"></a>
<a id="Vercel_AI_Tencent_TencentOptions_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; set; }
```


Session token for temporary Tencent Cloud credentials, sent as `X-TC-Token`. Defaults to the `TENCENTCLOUD_SESSION_TOKEN` environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L28)

<a id="Vercel_AI_Tencent_TencentOptions_SubAppId_"></a>
<a id="Vercel_AI_Tencent_TencentOptions_SubAppId"></a>

### SubAppId


```csharp
public string? SubAppId { get; set; }
```


VOD application id (`SubAppId`), used by VOD video models. Defaults to the `TENCENTCLOUD_VOD_SUB_APP_ID` environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L31)

<a id="Vercel_AI_Tencent_TencentOptions_VodBaseUrl_"></a>
<a id="Vercel_AI_Tencent_TencentOptions_VodBaseUrl"></a>

### VodBaseUrl


```csharp
public string? VodBaseUrl { get; set; }
```


VOD API base URL. Defaults to `https://vod.intl.tencentcloudapi.com`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L37)
