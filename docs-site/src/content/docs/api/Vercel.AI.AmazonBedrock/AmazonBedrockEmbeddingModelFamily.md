---
title: "AmazonBedrockEmbeddingModelFamily"
description: "Amazon Bedrock embedding model family. It selects the request format and the batch size."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockEmbeddingModelFamily"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelFamily"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public enum AmazonBedrockEmbeddingModelFamily
```


Amazon Bedrock embedding model family. It selects the request format and the batch size.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModelSettings.cs#L8)

## Fields

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelFamily_Cohere"></a>

### Cohere


```csharp
Cohere = 1
```


Cohere. Up to 96 values per call.

**Value:** [AmazonBedrockEmbeddingModelFamily](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModelSettings.cs#L14)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelFamily_Nova"></a>

### Nova


```csharp
Nova = 2
```


Amazon Nova. One value per call.

**Value:** [AmazonBedrockEmbeddingModelFamily](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModelSettings.cs#L17)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelFamily_Titan"></a>

### Titan


```csharp
Titan = 0
```


Amazon Titan. One value per call.

**Value:** [AmazonBedrockEmbeddingModelFamily](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModelSettings.cs#L11)
