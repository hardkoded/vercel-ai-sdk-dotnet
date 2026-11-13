---
title: "InvalidToolApproval"
description: "An approved tool call whose input failed validation. Maps to a collected approval with an error."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidToolApproval"
  hidden: true
---

<a id="Vercel_AI_GenerateText_InvalidToolApproval"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class InvalidToolApproval
```


An approved tool call whose input failed validation. Maps to a collected approval with an `error`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L13)

## Constructors

<a id="Vercel_AI_GenerateText_InvalidToolApproval__ctor_"></a>
<a id="Vercel_AI_GenerateText_InvalidToolApproval__ctor_Vercel_AI_GenerateText_CollectedToolApproval_Vercel_AI_Util_InvalidToolInputError_"></a>

### InvalidToolApproval(CollectedToolApproval, InvalidToolInputError)


```csharp
public InvalidToolApproval(CollectedToolApproval approval, InvalidToolInputError error)
```


Creates the entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approval` | [CollectedToolApproval](/api/Vercel.AI.GenerateText/CollectedToolApproval/) |  |
| `error` | [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L16)

## Properties

<a id="Vercel_AI_GenerateText_InvalidToolApproval_Approval_"></a>
<a id="Vercel_AI_GenerateText_InvalidToolApproval_Approval"></a>

### Approval


```csharp
public CollectedToolApproval Approval { get; }
```


The approval that was collected from the message history.

**Value:** [CollectedToolApproval](/api/Vercel.AI.GenerateText/CollectedToolApproval/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L23)

<a id="Vercel_AI_GenerateText_InvalidToolApproval_Error_"></a>
<a id="Vercel_AI_GenerateText_InvalidToolApproval_Error"></a>

### Error


```csharp
public InvalidToolInputError Error { get; }
```


Why the input is invalid.

**Value:** [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L26)
