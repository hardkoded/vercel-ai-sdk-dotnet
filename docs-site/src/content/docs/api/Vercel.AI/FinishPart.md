---
title: "FinishPart"
description: "The full stream finished."
editUrl: false
prev: false
next: false
sidebar:
  label: "FinishPart"
  hidden: true
---

<a id="Vercel_AI_FinishPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class FinishPart : TextStreamPart
```


The full stream finished.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1453)

## Constructors

<a id="Vercel_AI_FinishPart__ctor_"></a>
<a id="Vercel_AI_FinishPart__ctor_Vercel_AI_Provider_FinishReason_Vercel_AI_Provider_LanguageModelUsage_"></a>

### FinishPart(FinishReason, LanguageModelUsage)


```csharp
public FinishPart(FinishReason finishReason, LanguageModelUsage usage)
```


Creates a finish part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1456)

## Properties

<a id="Vercel_AI_FinishPart_FinishReason_"></a>
<a id="Vercel_AI_FinishPart_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1464)

<a id="Vercel_AI_FinishPart_Usage_"></a>
<a id="Vercel_AI_FinishPart_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Total usage.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1467)
