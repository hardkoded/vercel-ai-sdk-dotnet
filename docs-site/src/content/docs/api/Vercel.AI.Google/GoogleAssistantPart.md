---
title: "GoogleAssistantPart"
description: "An assistant content part."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleAssistantPart"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleAssistantPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public abstract class GoogleAssistantPart
```


An assistant content part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [GoogleFileAssistantPart](/api/Vercel.AI.Google/GoogleFileAssistantPart/), [GoogleReasoningAssistantPart](/api/Vercel.AI.Google/GoogleReasoningAssistantPart/), [GoogleTextAssistantPart](/api/Vercel.AI.Google/GoogleTextAssistantPart/), [GoogleToolCallAssistantPart](/api/Vercel.AI.Google/GoogleToolCallAssistantPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L150)

## Properties

<a id="Vercel_AI_Google_GoogleAssistantPart_ProviderOptions_"></a>
<a id="Vercel_AI_Google_GoogleAssistantPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options keyed by namespace.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L153)
