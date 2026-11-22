---
title: "AmazonBedrockChatModelSettings"
description: "Settings for AmazonBedrockLanguageModel. Maps to AmazonBedrockChatModelSettings."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockChatModelSettings"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockChatModelSettings"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockChatModelSettings
```


Settings for [AmazonBedrockLanguageModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockLanguageModel/). Maps to `AmazonBedrockChatModelSettings`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockChatModelSettings.cs#L15)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockChatModelSettings_ModelFamily_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockChatModelSettings_ModelFamily"></a>

### ModelFamily


```csharp
public AmazonBedrockChatModelFamily? ModelFamily { get; set; }
```


The underlying model family. Set it when the model id does not identify the underlying model,
such as an application inference profile ARN.

**Value:** `AmazonBedrockChatModelFamily?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockChatModelSettings.cs#L21)
