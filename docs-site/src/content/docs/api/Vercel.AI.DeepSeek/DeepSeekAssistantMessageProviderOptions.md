---
title: "DeepSeekAssistantMessageProviderOptions"
description: "The deepseek provider options of an assistant message. Maps to DeepSeekAssistantMessageProviderOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekAssistantMessageProviderOptions"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekAssistantMessageProviderOptions"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekAssistantMessageProviderOptions : DeepSeekMessageProviderOptions
```


The `deepseek` provider options of an assistant message. Maps to `DeepSeekAssistantMessageProviderOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DeepSeekMessageProviderOptions](/api/Vercel.AI.DeepSeek/DeepSeekMessageProviderOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L386)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekAssistantMessageProviderOptions_Prefix_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekAssistantMessageProviderOptions_Prefix"></a>

### Prefix


```csharp
public bool? Prefix { get; set; }
```


Whether the assistant message content is a prefix that DeepSeek should continue. This beta feature is only supported
on the final assistant message when using a beta base URL.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L392)
