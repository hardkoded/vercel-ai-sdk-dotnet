---
title: "AzureImageModelMaiOptions"
description: "MAI image options, read from providerOptions.azure. Maps to AzureImageModelMaiOptions and azureMaiImageModelOptionsShape in azure-mai-image-model-options.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureImageModelMaiOptions"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureImageModelMaiOptions"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public class AzureImageModelMaiOptions
```


MAI image options, read from `providerOptions.azure`. Maps to `AzureImageModelMaiOptions` and
`azureMaiImageModelOptionsShape` in `azure-mai-image-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AzureImageModelOptions](/api/Vercel.AI.Azure/AzureImageModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureMaiImageModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Azure_AzureImageModelMaiOptions_AutoAspectRatio_"></a>
<a id="Vercel_AI_Azure_AzureImageModelMaiOptions_AutoAspectRatio"></a>

### AutoAspectRatio


```csharp
public bool? AutoAspectRatio { get; set; }
```


Let the model pick the output aspect ratio from the prompt and any reference images. MAI-Image-2.6 models only.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureMaiImageModelOptions.cs#L16)

<a id="Vercel_AI_Azure_AzureImageModelMaiOptions_WebGrounding_"></a>
<a id="Vercel_AI_Azure_AzureImageModelMaiOptions_WebGrounding"></a>

### WebGrounding


```csharp
public bool? WebGrounding { get; set; }
```


Ground generation in Bing web search results. MAI-Image-2.6 models only.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureMaiImageModelOptions.cs#L19)
