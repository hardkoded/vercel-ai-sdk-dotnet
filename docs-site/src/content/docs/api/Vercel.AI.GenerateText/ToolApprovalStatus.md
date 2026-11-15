---
title: "ToolApprovalStatus"
description: "The approval status of a tool call: not-applicable (no approval needed), approved (approved automatically), denied (denied automatically), or user-approval (..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalStatus"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalStatus"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolApprovalStatus
```


The approval status of a tool call: `not-applicable` (no approval needed), `approved` (approved automatically),
`denied` (denied automatically), or `user-approval` (the user must approve). Maps to `ToolApprovalStatus`.
For `approved` and `denied`, the reason is emitted on the approval response. For `user-approval`, it is
emitted on the approval request so it can be shown to the approver. A function that returns &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt;
has the effect of `not-applicable`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L18)

## Constructors

<a id="Vercel_AI_GenerateText_ToolApprovalStatus__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus__ctor_System_String_System_String_"></a>

### ToolApprovalStatus(string, string?)


```csharp
public ToolApprovalStatus(string type, string? reason = null)
```


Creates a status. &lt;code class="paramref"&gt;type&lt;/code&gt; is one of the four type constants of this class.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L33)

## Fields

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_ApprovedType"></a>

### ApprovedType


```csharp
public const string ApprovedType = "approved"
```


The tool is approved automatically.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L24)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_DeniedType"></a>

### DeniedType


```csharp
public const string DeniedType = "denied"
```


The tool is denied automatically.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L27)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_NotApplicableType"></a>

### NotApplicableType


```csharp
public const string NotApplicableType = "not-applicable"
```


The tool does not require approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L21)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_UserApprovalType"></a>

### UserApprovalType


```csharp
public const string UserApprovalType = "user-approval"
```


The tool requires user approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L30)

## Properties

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Approved_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Approved"></a>

### Approved


```csharp
public static ToolApprovalStatus Approved { get; }
```


The status without a reason: `approved`.

**Value:** [ToolApprovalStatus](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L53)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Denied_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Denied"></a>

### Denied


```csharp
public static ToolApprovalStatus Denied { get; }
```


The status without a reason: `denied`.

**Value:** [ToolApprovalStatus](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L56)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_NotApplicable_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_NotApplicable"></a>

### NotApplicable


```csharp
public static ToolApprovalStatus NotApplicable { get; }
```


The status without a reason: `not-applicable`.

**Value:** [ToolApprovalStatus](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L50)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Reason_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Why the call was approved, denied, or needs approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L65)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Type_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_Type"></a>

### Type


```csharp
public string Type { get; }
```


One of `not-applicable`, `approved`, `denied`, and `user-approval`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L62)

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_UserApproval_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_UserApproval"></a>

### UserApproval


```csharp
public static ToolApprovalStatus UserApproval { get; }
```


The status without a reason: `user-approval`.

**Value:** [ToolApprovalStatus](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L59)

## Operators

<a id="Vercel_AI_GenerateText_ToolApprovalStatus_op_Implicit_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalStatus_op_Implicit_System_String__Vercel_AI_GenerateText_ToolApprovalStatus"></a>

### implicit operator ToolApprovalStatus(string)


```csharp
public static implicit operator ToolApprovalStatus(string type)
```


Converts a status string, such as `user-approval`, to a status without a reason.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ToolApprovalStatus](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L68)
