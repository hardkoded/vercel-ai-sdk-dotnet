---
title: "LanguageReasoningCapabilities"
description: "Upstream ModelCapabilities.language.reasoning."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageReasoningCapabilities"
  hidden: true
---

<a id="Vercel_AI_LanguageReasoningCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class LanguageReasoningCapabilities
```


Upstream `ModelCapabilities.language.reasoning`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L137)

## Properties

<a id="Vercel_AI_LanguageReasoningCapabilities_Efforts_"></a>
<a id="Vercel_AI_LanguageReasoningCapabilities_Efforts"></a>

### Efforts


```csharp
public IReadOnlyList<string>? Efforts { get; init; }
```


Null means the provider does not declare an exhaustive list.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L143)

<a id="Vercel_AI_LanguageReasoningCapabilities_Support_"></a>
<a id="Vercel_AI_LanguageReasoningCapabilities_Support"></a>

### Support


```csharp
public ModelCapability Support { get; init; }
```


Reasoning support.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L140)
