---
title: "TencentCloudCredentials"
description: "Credentials for Tencent Cloud API 3.0. Upstream TencentCloudCredentials."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentCloudCredentials"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentCloudCredentials"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public class TencentCloudCredentials
```


Credentials for Tencent Cloud API 3.0. Upstream `TencentCloudCredentials`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [TencentVodCredentials](/api/Vercel.AI.Tencent/TencentVodCredentials/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L16)

## Properties

<a id="Vercel_AI_Tencent_TencentCloudCredentials_SecretId_"></a>
<a id="Vercel_AI_Tencent_TencentCloudCredentials_SecretId"></a>

### SecretId


```csharp
public string SecretId { get; set; }
```


API SecretId.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L19)

<a id="Vercel_AI_Tencent_TencentCloudCredentials_SecretKey_"></a>
<a id="Vercel_AI_Tencent_TencentCloudCredentials_SecretKey"></a>

### SecretKey


```csharp
public string SecretKey { get; set; }
```


API SecretKey.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L22)

<a id="Vercel_AI_Tencent_TencentCloudCredentials_SessionToken_"></a>
<a id="Vercel_AI_Tencent_TencentCloudCredentials_SessionToken"></a>

### SessionToken


```csharp
public string? SessionToken { get; set; }
```


Session token for temporary credentials, sent as `X-TC-Token`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentCloudApi.cs#L25)
