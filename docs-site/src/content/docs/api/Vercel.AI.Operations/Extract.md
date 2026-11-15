---
title: "Extract"
description: "Extracts typed field values from one shared state. Maps to experimentalextract."
editUrl: false
prev: false
next: false
sidebar:
  label: "Extract"
  hidden: true
---

<a id="Vercel_AI_Operations_Extract"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class Extract
```


Extracts typed field values from one shared state. Maps to `experimental_extract`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L75)

## Methods

<a id="Vercel_AI_Operations_Extract_ExtractAsync_"></a>
<a id="Vercel_AI_Operations_Extract_ExtractAsync_Vercel_AI_Operations_ExtractRequest_System_Threading_CancellationToken_"></a>

### ExtractAsync(ExtractRequest, CancellationToken)


```csharp
public static Task<ExtractionResult> ExtractAsync(ExtractRequest request, CancellationToken cancellationToken = default)
```


Extracts every field in one model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [ExtractRequest](/api/Vercel.AI.Operations/ExtractRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ExtractionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L78)
