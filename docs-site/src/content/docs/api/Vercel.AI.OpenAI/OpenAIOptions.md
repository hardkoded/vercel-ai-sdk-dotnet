---
title: "OpenAIOptions"
description: "OpenAI provider settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIOptions : OpenAICompatibleOptions
```


OpenAI provider settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L18)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIOptions__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIOptions__ctor"></a>

### OpenAIOptions()


```csharp
public OpenAIOptions()
```


Creates options aimed at `https://api.openai.com/v1` unless `OPENAI_BASE_URL` is set.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L26)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIOptions_BaseUrl_"></a>
<a id="Vercel_AI_OpenAI_OpenAIOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API origin. An assigned value wins over `OPENAI_BASE_URL`.
A blank value is rejected when the provider is created.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L38)

<a id="Vercel_AI_OpenAI_OpenAIOptions_BatchResultDownloads_"></a>
<a id="Vercel_AI_OpenAI_OpenAIOptions_BatchResultDownloads"></a>

### BatchResultDownloads


```csharp
public BatchResultDownloads? BatchResultDownloads { get; set; }
```


Settings for downloading JSON Lines batch results.

**Value:** [BatchResultDownloads](/api/Vercel.AI.ProviderUtils/BatchResultDownloads/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L23)

<a id="Vercel_AI_OpenAI_OpenAIOptions_Organization_"></a>
<a id="Vercel_AI_OpenAI_OpenAIOptions_Organization"></a>

### Organization


```csharp
public string? Organization { get; set; }
```


Value of the `OpenAI-Organization` header.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L61)

<a id="Vercel_AI_OpenAI_OpenAIOptions_Project_"></a>
<a id="Vercel_AI_OpenAI_OpenAIOptions_Project"></a>

### Project


```csharp
public string? Project { get; set; }
```


Value of the `OpenAI-Project` header.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L64)

<a id="Vercel_AI_OpenAI_OpenAIOptions_UseResponsesApi_"></a>
<a id="Vercel_AI_OpenAI_OpenAIOptions_UseResponsesApi"></a>

### UseResponsesApi


```csharp
public bool UseResponsesApi { get; set; }
```


When true, [LanguageModel(string)](/api/Vercel.AI.OpenAI/OpenAIProvider/#Vercel_AI_OpenAI_OpenAIProvider_LanguageModel_System_String_) uses the Responses API.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L58)
