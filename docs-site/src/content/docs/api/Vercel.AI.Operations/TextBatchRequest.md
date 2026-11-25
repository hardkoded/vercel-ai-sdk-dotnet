---
title: "TextBatchRequest"
description: "One text generation request within a batch. Maps to TextBatchRequest."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextBatchRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_TextBatchRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TextBatchRequest : BatchRequest
```


One text generation request within a batch. Maps to `TextBatchRequest`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchRequest](/api/Vercel.AI.Operations/BatchRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L129)

## Constructors

<a id="Vercel_AI_Operations_TextBatchRequest__ctor_"></a>
<a id="Vercel_AI_Operations_TextBatchRequest__ctor_System_String_System_String_"></a>

### TextBatchRequest(string, string)


```csharp
public TextBatchRequest(string id, string model)
```


Creates a text request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L132)
