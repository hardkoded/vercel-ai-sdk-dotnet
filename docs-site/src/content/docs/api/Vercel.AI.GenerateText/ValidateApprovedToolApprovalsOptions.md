---
title: "ValidateApprovedToolApprovalsOptions"
description: "The options of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ValidateApprovedToolApprovalsOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ValidateApprovedToolApprovalsOptions
```


The options of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L30)

## Constructors

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions__ctor_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_CollectedToolApproval__"></a>

### `ValidateApprovedToolApprovalsOptions(IReadOnlyList<CollectedToolApproval>)`


```csharp
public ValidateApprovedToolApprovalsOptions(IReadOnlyList<CollectedToolApproval> approvedToolApprovals)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvedToolApprovals` | [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L33)

## Properties

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ApprovedToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ApprovedToolApprovals"></a>

### ApprovedToolApprovals


```csharp
public IReadOnlyList<CollectedToolApproval> ApprovedToolApprovals { get; }
```


The approvals that the history reports as approved.

**Value:** [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L39)

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_Messages_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; init; }
```


Messages that were sent to the language model, without the instructions.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L48)

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_RefineToolInput_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; init; }
```


Refines the parsed input of a tool, keyed by tool name. Maps to `refineToolInput`.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L57)

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApproval_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApproval"></a>

### ToolApproval


```csharp
public ToolApprovalConfiguration? ToolApproval { get; init; }
```


User-defined approval configuration. The approval policy is evaluated again on the approved input.

**Value:** [ToolApprovalConfiguration](/api/Vercel.AI.GenerateText/ToolApprovalConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L45)

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApprovalSecret_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApprovalSecret"></a>

### ToolApprovalSecret


```csharp
public string? ToolApprovalSecret { get; init; }
```


Secret for the HMAC signature of the approval requests. Maps to `toolApprovalSecret` given as a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L51)

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApprovalSecretBytes_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApprovalSecretBytes"></a>

### ToolApprovalSecretBytes


```csharp
public byte[]? ToolApprovalSecretBytes { get; init; }
```


The same secret given as bytes. It is used when [ToolApprovalSecret](#Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_ToolApprovalSecret) is not set.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L54)

<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_Tools_"></a>
<a id="Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; init; }
```


The available tools.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L42)
