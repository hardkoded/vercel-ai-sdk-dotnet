---
title: "DeepSeekReasoningCapabilities"
description: "Reasoning support of a DeepSeek language model. Upstream LanguageCapabilities.reasoning."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekReasoningCapabilities"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekReasoningCapabilities
```


Reasoning support of a DeepSeek language model. Upstream `LanguageCapabilities.reasoning`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L60)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_AssistantHistory_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_AssistantHistory"></a>

### AssistantHistory


```csharp
public bool AssistantHistory { get; init; }
```


Upstream `assistantHistory`: whether assistant turns must carry `reasoning_content`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L75)

<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_Configurable_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_Configurable"></a>

### Configurable


```csharp
public bool Configurable { get; init; }
```


Upstream `configurable`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L69)

<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_Efforts_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_Efforts"></a>

### Efforts


```csharp
public IReadOnlyList<string>? Efforts { get; init; }
```


Upstream `efforts`. Null when not declared.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L66)

<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_EnabledByDefault_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_EnabledByDefault"></a>

### EnabledByDefault


```csharp
public bool EnabledByDefault { get; init; }
```


Upstream `enabledByDefault`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L72)

<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_Support_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekReasoningCapabilities_Support"></a>

### Support


```csharp
public ModelCapability Support { get; init; }
```


Upstream `support`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L63)
