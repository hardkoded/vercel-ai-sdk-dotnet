---
title: "CerebrasPrediction"
description: "Predicted output content. Maps to the prediction option of CerebrasLanguageModelChatOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasPrediction"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasPrediction"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public sealed class CerebrasPrediction
```


Predicted output content. Maps to the `prediction` option of `CerebrasLanguageModelChatOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L64)

## Properties

<a id="Vercel_AI_Cerebras_CerebrasPrediction_Content_"></a>
<a id="Vercel_AI_Cerebras_CerebrasPrediction_Content"></a>

### Content


```csharp
public string? Content { get; set; }
```


The predicted content as one string. Ignored when [ContentParts](#Vercel_AI_Cerebras_CerebrasPrediction_ContentParts) is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L67)

<a id="Vercel_AI_Cerebras_CerebrasPrediction_ContentParts_"></a>
<a id="Vercel_AI_Cerebras_CerebrasPrediction_ContentParts"></a>

### ContentParts


```csharp
public IReadOnlyList<string>? ContentParts { get; set; }
```


The predicted content as text parts.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasLanguageModelChatOptions.cs#L70)
