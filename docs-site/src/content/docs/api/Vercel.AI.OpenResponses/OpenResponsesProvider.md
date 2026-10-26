---
title: "OpenResponsesProvider"
description: "Open Responses provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesProvider"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesProvider : ProviderBase
```


Open Responses provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L61)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenResponses_OpenResponsesOptions_"></a>

### OpenResponsesProvider(HttpClient, OpenResponsesOptions?)


```csharp
public OpenResponsesProvider(HttpClient httpClient, OpenResponsesOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenResponsesOptions](/api/Vercel.AI.OpenResponses/OpenResponsesOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L67)

## Fields

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_ProviderName"></a>

### ProviderName


```csharp
public const string ProviderName = "open-responses"
```


Default provider name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L64)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_ExtensionRegistry_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_ExtensionRegistry"></a>

### ExtensionRegistry


```csharp
public OpenResponsesExtensionRegistry ExtensionRegistry { get; }
```


The registered extensions, indexed.

**Value:** [OpenResponsesExtensionRegistry](/api/Vercel.AI.OpenResponses/OpenResponsesExtensionRegistry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L79)

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_Http_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L82)

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_Options_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_Options"></a>

### Options


```csharp
public OpenResponsesOptions Options { get; }
```


Options.

**Value:** [OpenResponsesOptions](/api/Vercel.AI.OpenResponses/OpenResponsesOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L76)

## Methods

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_Create_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_Create_Vercel_AI_OpenResponses_OpenResponsesOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenResponsesOptions?, HttpMessageHandler?)


```csharp
public static OpenResponsesProvider Create(OpenResponsesOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenResponsesOptions](/api/Vercel.AI.OpenResponses/OpenResponsesOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [OpenResponsesProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L85)

<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_LanguageModel_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L92)
