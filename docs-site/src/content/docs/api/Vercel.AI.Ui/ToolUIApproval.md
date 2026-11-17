---
title: "ToolUIApproval"
description: "The approval of a tool call in a ToolUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolUIApproval"
  hidden: true
---

<a id="Vercel_AI_Ui_ToolUIApproval"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class ToolUIApproval
```


The approval of a tool call in a [ToolUIPart](/api/Vercel.AI.Ui/ToolUIPart/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L279)

## Constructors

<a id="Vercel_AI_Ui_ToolUIApproval__ctor_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval__ctor_System_String_"></a>

### ToolUIApproval(string)


```csharp
public ToolUIApproval(string id)
```


Creates an approval.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L282)

## Properties

<a id="Vercel_AI_Ui_ToolUIApproval_Approved_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_Approved"></a>

### Approved


```csharp
public bool? Approved { get; init; }
```


Whether the call was approved. Null while the approval is requested.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L291)

<a id="Vercel_AI_Ui_ToolUIApproval_Descriptor_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_Descriptor"></a>

### Descriptor


```csharp
public JsonElement? Descriptor { get; init; }
```


A descriptor of the approval, as the UI keeps it. Maps to `descriptor`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L303)

<a id="Vercel_AI_Ui_ToolUIApproval_Id_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_Id"></a>

### Id


```csharp
public string Id { get; }
```


The approval id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L288)

<a id="Vercel_AI_Ui_ToolUIApproval_InputSchemaInput_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_InputSchemaInput"></a>

### InputSchemaInput


```csharp
public JsonElement? InputSchemaInput { get; init; }
```


The input of the call before its input schema transformed it. Maps to `inputSchemaInput`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L309)

<a id="Vercel_AI_Ui_ToolUIApproval_IsAutomatic_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_IsAutomatic"></a>

### IsAutomatic


```csharp
public bool? IsAutomatic { get; init; }
```


Whether the approval was decided automatically.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L300)

<a id="Vercel_AI_Ui_ToolUIApproval_Reason_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_Reason"></a>

### Reason


```csharp
public string? Reason { get; init; }
```


Why the call was approved or denied.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L297)

<a id="Vercel_AI_Ui_ToolUIApproval_RequestReason_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_RequestReason"></a>

### RequestReason


```csharp
public string? RequestReason { get; init; }
```


Why the approval was requested.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L294)

<a id="Vercel_AI_Ui_ToolUIApproval_Signature_"></a>
<a id="Vercel_AI_Ui_ToolUIApproval_Signature"></a>

### Signature


```csharp
public string? Signature { get; init; }
```


HMAC-SHA256 signature that binds the approval request to its tool call. Maps to `signature`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L306)
