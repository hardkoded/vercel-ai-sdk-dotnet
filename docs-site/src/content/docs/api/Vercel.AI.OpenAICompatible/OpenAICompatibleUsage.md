---
title: "OpenAICompatibleUsage"
description: "Chat usage plus prediction-token counts stored as provider metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleUsage"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleUsage
```


Chat usage plus prediction-token counts stored as provider metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L626)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage__ctor_Vercel_AI_Provider_LanguageModelUsage_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### OpenAICompatibleUsage(LanguageModelUsage, int?, int?)


```csharp
public OpenAICompatibleUsage(LanguageModelUsage usage, int? acceptedPredictionTokens, int? rejectedPredictionTokens)
```


Creates converted usage.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `acceptedPredictionTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `rejectedPredictionTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L632)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_AcceptedPredictionTokens_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_AcceptedPredictionTokens"></a>

### AcceptedPredictionTokens


```csharp
public int? AcceptedPredictionTokens { get; }
```


Accepted prediction tokens, when reported.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L643)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_Missing_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_Missing"></a>

### Missing


```csharp
public static OpenAICompatibleUsage Missing { get; }
```


Usage when the provider omitted the object.

**Value:** [OpenAICompatibleUsage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L629)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_RejectedPredictionTokens_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_RejectedPredictionTokens"></a>

### RejectedPredictionTokens


```csharp
public int? RejectedPredictionTokens { get; }
```


Rejected prediction tokens, when reported.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L646)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_Usage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleUsage_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Token counts.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L640)
