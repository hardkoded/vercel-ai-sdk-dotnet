---
title: "AzureImageModelOptions"
description: "Azure image options, read from providerOptions.azure. Maps to AzureImageModelOptions and azureImageModelOptions in azure-image-model-options.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureImageModelOptions"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureImageModelOptions"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureImageModelOptions : AzureImageModelMaiOptions
```


Azure image options, read from `providerOptions.azure`. Maps to `AzureImageModelOptions` and
`azureImageModelOptions` in `azure-image-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AzureImageModelMaiOptions](/api/Vercel.AI.Azure/AzureImageModelMaiOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureImageModelOptions.cs#L14)

## Properties

<a id="Vercel_AI_Azure_AzureImageModelOptions_Api_"></a>
<a id="Vercel_AI_Azure_AzureImageModelOptions_Api"></a>

### Api


```csharp
public string? Api { get; set; }
```


API to use: `openai` or `mai`. Defaults to MAI for MAI-Image models, OpenAI otherwise.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureImageModelOptions.cs#L17)

## Methods

<a id="Vercel_AI_Azure_AzureImageModelOptions_IsMAIImageModel_"></a>
<a id="Vercel_AI_Azure_AzureImageModelOptions_IsMAIImageModel_System_String_"></a>

### IsMAIImageModel(string)


```csharp
public static bool IsMAIImageModel(string modelId)
```


True when &lt;code class="paramref"&gt;modelId&lt;/code&gt; is a MAI-Image model. Maps to `isMAIImageModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureImageModelOptions.cs#L20)
