---
title: "FinishReason"
description: "Why a language-model call stopped."
editUrl: false
prev: false
next: false
sidebar:
  label: "FinishReason"
  hidden: true
---

<a id="Vercel_AI_Provider_FinishReason"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public enum FinishReason
```


Why a language-model call stopped.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L13)

## Fields

<a id="Vercel_AI_Provider_FinishReason_ContentFilter"></a>

### ContentFilter


```csharp
ContentFilter = 2
```


The provider filtered the content.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L22)

<a id="Vercel_AI_Provider_FinishReason_Error"></a>

### Error


```csharp
Error = 4
```


The call failed.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L28)

<a id="Vercel_AI_Provider_FinishReason_Interrupted"></a>

### Interrupted


```csharp
Interrupted = 6
```


The provider stream ended without a completion signal. Output may be incomplete.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L34)

<a id="Vercel_AI_Provider_FinishReason_Length"></a>

### Length


```csharp
Length = 1
```


The output token limit was reached.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L19)

<a id="Vercel_AI_Provider_FinishReason_Other"></a>

### Other


```csharp
Other = 5
```


A provider-specific reason.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L31)

<a id="Vercel_AI_Provider_FinishReason_Stop"></a>

### Stop


```csharp
Stop = 0
```


The model finished naturally.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L16)

<a id="Vercel_AI_Provider_FinishReason_ToolCalls"></a>

### ToolCalls


```csharp
ToolCalls = 3
```


The model called one or more tools.

**Value:** [FinishReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L25)
