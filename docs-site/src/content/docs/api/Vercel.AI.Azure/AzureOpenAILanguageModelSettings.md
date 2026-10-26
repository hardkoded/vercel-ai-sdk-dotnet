---
title: "AzureOpenAILanguageModelSettings"
description: "Settings for an Azure OpenAI language model."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureOpenAILanguageModelSettings"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureOpenAILanguageModelSettings"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureOpenAILanguageModelSettings
```


Settings for an Azure OpenAI language model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L44)

## Properties

<a id="Vercel_AI_Azure_AzureOpenAILanguageModelSettings_ModelId_"></a>
<a id="Vercel_AI_Azure_AzureOpenAILanguageModelSettings_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Underlying model ID used for reported identity and capability detection.
Defaults to the deployment ID. Requests always use the deployment ID.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L50)
