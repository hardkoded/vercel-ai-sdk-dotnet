---
title: "OpenResponsesItemMode"
description: "Whether an item is decoded from a generate response or from a stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesItemMode"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesItemMode"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public enum OpenResponsesItemMode
```


Whether an item is decoded from a generate response or from a stream.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L30)

## Fields

<a id="Vercel_AI_OpenResponses_OpenResponsesItemMode_Generate"></a>

### Generate


```csharp
Generate = 0
```


A `DoGenerateAsync` response.

**Value:** [OpenResponsesItemMode](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L33)

<a id="Vercel_AI_OpenResponses_OpenResponsesItemMode_Stream"></a>

### Stream


```csharp
Stream = 1
```


A `DoStreamAsync` stream.

**Value:** [OpenResponsesItemMode](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L36)
