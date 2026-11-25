---
title: "StreamTextOnErrorResult"
description: "The result of OnErrorRetry. Maps to { retry?: boolean }."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextOnErrorResult"
  hidden: true
---

<a id="Vercel_AI_StreamTextOnErrorResult"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextOnErrorResult
```


The result of [OnErrorRetry](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnErrorRetry). Maps to `{ retry?: boolean }`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2226)

## Properties

<a id="Vercel_AI_StreamTextOnErrorResult_Retry_"></a>
<a id="Vercel_AI_StreamTextOnErrorResult_Retry"></a>

### Retry


```csharp
public bool Retry { get; init; }
```


Whether to retry the current model step. Maps to `retry`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2229)
