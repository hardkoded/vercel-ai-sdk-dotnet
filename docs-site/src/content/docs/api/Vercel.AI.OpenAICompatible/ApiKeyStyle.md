---
title: "ApiKeyStyle"
description: "How the API key is sent."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiKeyStyle"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public enum ApiKeyStyle
```


How the API key is sent.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L12)

## Fields

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle_ApiKeyHeader"></a>

### ApiKeyHeader


```csharp
ApiKeyHeader = 1
```


A custom header such as `api-key`.

**Value:** [ApiKeyStyle](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L18)

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle_Bearer"></a>

### Bearer


```csharp
Bearer = 0
```


`Authorization: Bearer`.

**Value:** [ApiKeyStyle](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L15)

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle_CustomHeader"></a>

### CustomHeader


```csharp
CustomHeader = 5
```


A named header whose value is the raw key.

**Value:** [ApiKeyStyle](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L30)

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle_Key"></a>

### Key


```csharp
Key = 3
```


`Authorization: Key`.

**Value:** [ApiKeyStyle](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L24)

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle_RawAuthorization"></a>

### RawAuthorization


```csharp
RawAuthorization = 4
```


The raw key as the `Authorization` value.

**Value:** [ApiKeyStyle](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L27)

<a id="Vercel_AI_OpenAICompatible_ApiKeyStyle_Token"></a>

### Token


```csharp
Token = 2
```


`Authorization: Token`.

**Value:** [ApiKeyStyle](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L21)
