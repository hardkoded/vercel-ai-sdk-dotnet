---
title: "ToolApprovalEntry"
description: "The approval setting of one tool: a status or a function that produces a status at runtime."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalEntry"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalEntry"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolApprovalEntry
```


The approval setting of one tool: a status or a function that produces a status at runtime.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L128)

## Constructors

<a id="Vercel_AI_GenerateText_ToolApprovalEntry__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalEntry__ctor_Vercel_AI_GenerateText_SingleToolApprovalFunction_"></a>

### ToolApprovalEntry(SingleToolApprovalFunction)


```csharp
public ToolApprovalEntry(SingleToolApprovalFunction function)
```


Creates an entry whose status is produced for each call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `function` | [SingleToolApprovalFunction](/api/Vercel.AI.GenerateText/SingleToolApprovalFunction/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L137)

<a id="Vercel_AI_GenerateText_ToolApprovalEntry__ctor_Vercel_AI_GenerateText_ToolApprovalStatus_"></a>

### ToolApprovalEntry(ToolApprovalStatus)


```csharp
public ToolApprovalEntry(ToolApprovalStatus status)
```


Creates an entry with a fixed status.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [ToolApprovalStatus](/api/Vercel.AI.GenerateText/ToolApprovalStatus/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L131)

## Properties

<a id="Vercel_AI_GenerateText_ToolApprovalEntry_Function_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalEntry_Function"></a>

### Function


```csharp
public SingleToolApprovalFunction? Function { get; }
```


The function that produces the status, or null when [Status](#Vercel_AI_GenerateText_ToolApprovalEntry_Status) is set.

**Value:** [SingleToolApprovalFunction](/api/Vercel.AI.GenerateText/SingleToolApprovalFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L146)

<a id="Vercel_AI_GenerateText_ToolApprovalEntry_Status_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalEntry_Status"></a>

### Status


```csharp
public ToolApprovalStatus? Status { get; }
```


The fixed status, or null when [Function](#Vercel_AI_GenerateText_ToolApprovalEntry_Function) is set.

**Value:** [ToolApprovalStatus](/api/Vercel.AI.GenerateText/ToolApprovalStatus/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L143)

## Operators

<a id="Vercel_AI_GenerateText_ToolApprovalEntry_op_Implicit_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalEntry_op_Implicit_Vercel_AI_GenerateText_ToolApprovalStatus__Vercel_AI_GenerateText_ToolApprovalEntry"></a>

### implicit operator ToolApprovalEntry(ToolApprovalStatus)


```csharp
public static implicit operator ToolApprovalEntry(ToolApprovalStatus status)
```


Converts a status to an entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [ToolApprovalStatus](/api/Vercel.AI.GenerateText/ToolApprovalStatus/) |  |

**Returns:** [ToolApprovalEntry](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L149)
