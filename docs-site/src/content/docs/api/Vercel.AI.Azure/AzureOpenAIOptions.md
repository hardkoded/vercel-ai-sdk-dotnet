---
title: "AzureOpenAIOptions"
description: "Azure OpenAI settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureOpenAIOptions"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureOpenAIOptions"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureOpenAIOptions
```


Azure OpenAI settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L14)

## Properties

<a id="Vercel_AI_Azure_AzureOpenAIOptions_ApiKey_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit key. Falls back to `AZURE_API_KEY`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L26)

<a id="Vercel_AI_Azure_AzureOpenAIOptions_ApiVersion_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_ApiVersion"></a>

### ApiVersion


```csharp
public string ApiVersion { get; set; }
```


API version query parameter. Defaults to `v1`, also when set to null or empty.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L29)

<a id="Vercel_AI_Azure_AzureOpenAIOptions_BaseUrl_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string? BaseUrl { get; set; }
```


Resource host, for example `https://my-resource.openai.azure.com`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L17)

<a id="Vercel_AI_Azure_AzureOpenAIOptions_Headers_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string> Headers { get; }
```


Headers copied onto every request.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L40)

<a id="Vercel_AI_Azure_AzureOpenAIOptions_ResourceName_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_ResourceName"></a>

### ResourceName


```csharp
public string? ResourceName { get; set; }
```


Resource name used when [BaseUrl](#Vercel_AI_Azure_AzureOpenAIOptions_BaseUrl) is empty.
Must be a single DNS label (letters, digits, and hyphens).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L23)

<a id="Vercel_AI_Azure_AzureOpenAIOptions_TokenProvider_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_TokenProvider"></a>

### TokenProvider


```csharp
public Func<string>? TokenProvider { get; set; }
```


Microsoft Entra token provider. Each call receives a fresh bearer token.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L37)

<a id="Vercel_AI_Azure_AzureOpenAIOptions_UseDeploymentBasedUrls_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIOptions_UseDeploymentBasedUrls"></a>

### UseDeploymentBasedUrls


```csharp
public bool UseDeploymentBasedUrls { get; set; }
```


Uses the legacy `/openai/deployments/{deploymentId}{path}` routes instead of `/openai/v1{path}`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L34)
