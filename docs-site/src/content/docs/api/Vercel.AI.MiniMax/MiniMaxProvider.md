---
title: "MiniMaxProvider"
description: "MiniMax provider. Speaks the Anthropic Messages protocol at https://api.minimax.io/anthropic/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxProvider"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxProvider"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxProvider : ProviderBase
```


MiniMax provider. Speaks the Anthropic Messages protocol at `https://api.minimax.io/anthropic/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L25)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxProvider__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_MiniMax_MiniMaxOptions_"></a>

### MiniMaxProvider(HttpClient, MiniMaxOptions?)


```csharp
public MiniMaxProvider(HttpClient httpClient, MiniMaxOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L39)

## Fields

<a id="Vercel_AI_MiniMax_MiniMaxProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "MINIMAX_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L34)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.minimax.io/anthropic/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L31)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "minimax"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L28)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Options_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Options"></a>

### Options


```csharp
public MiniMaxOptions Options { get; }
```


Options.

**Value:** [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L59)

## Methods

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Chat_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Chat_System_String_"></a>

### Chat(string)


```csharp
public MiniMaxChatLanguageModel Chat(string modelId)
```


Creates a MiniMax chat model for text generation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MiniMaxChatLanguageModel](/api/Vercel.AI.MiniMax/MiniMaxChatLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L75)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Create_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Create_Vercel_AI_MiniMax_MiniMaxOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(MiniMaxOptions?, HttpMessageHandler?)


```csharp
public static MiniMaxProvider Create(MiniMaxOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [MiniMaxProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L62)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_LanguageModel_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Returns a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L69)
