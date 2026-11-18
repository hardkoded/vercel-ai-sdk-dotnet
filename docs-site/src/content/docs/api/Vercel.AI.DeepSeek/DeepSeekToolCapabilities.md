---
title: "DeepSeekToolCapabilities"
description: "Tool support of a DeepSeek language model. Upstream LanguageCapabilities.tools."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekToolCapabilities"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekToolCapabilities
```


Tool support of a DeepSeek language model. Upstream `LanguageCapabilities.tools`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L47)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities_Streaming_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities_Streaming"></a>

### Streaming


```csharp
public ModelCapability Streaming { get; init; }
```


Upstream `streaming`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L53)

<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities_Strict_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities_Strict"></a>

### Strict


```csharp
public ModelCapability Strict { get; init; }
```


Upstream `strict`. `Unknown` unless the endpoint declares strict tool calls.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L56)

<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities_Usage_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekToolCapabilities_Usage"></a>

### Usage


```csharp
public ModelCapability Usage { get; init; }
```


Upstream `usage`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L50)
