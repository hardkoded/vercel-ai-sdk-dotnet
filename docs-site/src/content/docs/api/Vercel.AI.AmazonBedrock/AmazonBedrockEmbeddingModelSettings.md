---
title: "AmazonBedrockEmbeddingModelSettings"
description: "Settings for AmazonBedrockEmbeddingModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockEmbeddingModelSettings"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelSettings"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockEmbeddingModelSettings
```


Settings for [AmazonBedrockEmbeddingModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModelSettings.cs#L21)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelSettings_ModelFamily_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelSettings_ModelFamily"></a>

### ModelFamily


```csharp
public AmazonBedrockEmbeddingModelFamily? ModelFamily { get; set; }
```


The embedding model family.
Set it when the model id does not name the model, such as an application inference profile ARN.
When unset, the family comes from the model id.

**Value:** `AmazonBedrockEmbeddingModelFamily?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModelSettings.cs#L28)
