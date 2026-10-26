---
title: "ToolInputEncoding"
description: "Encoding of the deltas that follow a tool-input-start part."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputEncoding"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolInputEncoding"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public enum ToolInputEncoding
```


Encoding of the deltas that follow a tool-input-start part.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1802)

## Fields

<a id="Vercel_AI_Provider_ToolInputEncoding_Json"></a>

### Json


```csharp
Json = 0
```


The deltas concatenate to JSON text.

**Value:** [ToolInputEncoding](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1805)

<a id="Vercel_AI_Provider_ToolInputEncoding_Text"></a>

### Text


```csharp
Text = 1
```


The deltas concatenate to a raw string, such as OpenAI custom-tool code or HTML.

**Value:** [ToolInputEncoding](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1808)
