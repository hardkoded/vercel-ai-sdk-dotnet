---
title: "ToolUIPartState"
description: "The state of a ToolUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolUIPartState"
  hidden: true
---

<a id="Vercel_AI_Ui_ToolUIPartState"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public enum ToolUIPartState
```


The state of a [ToolUIPart](/api/Vercel.AI.Ui/ToolUIPart/).

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L253)

## Fields

<a id="Vercel_AI_Ui_ToolUIPartState_ApprovalRequested"></a>

### ApprovalRequested


```csharp
ApprovalRequested = 2
```


The call waits for an approval. Maps to `'approval-requested'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L262)

<a id="Vercel_AI_Ui_ToolUIPartState_ApprovalResponded"></a>

### ApprovalResponded


```csharp
ApprovalResponded = 3
```


The approval was answered. Maps to `'approval-responded'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L265)

<a id="Vercel_AI_Ui_ToolUIPartState_InputAvailable"></a>

### InputAvailable


```csharp
InputAvailable = 1
```


The input is complete. Maps to `'input-available'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L259)

<a id="Vercel_AI_Ui_ToolUIPartState_InputStreaming"></a>

### InputStreaming


```csharp
InputStreaming = 0
```


The input is still streaming. Maps to `'input-streaming'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L256)

<a id="Vercel_AI_Ui_ToolUIPartState_OutputAvailable"></a>

### OutputAvailable


```csharp
OutputAvailable = 4
```


The output is available. Maps to `'output-available'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L268)

<a id="Vercel_AI_Ui_ToolUIPartState_OutputDenied"></a>

### OutputDenied


```csharp
OutputDenied = 6
```


The approval was denied. Maps to `'output-denied'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L274)

<a id="Vercel_AI_Ui_ToolUIPartState_OutputError"></a>

### OutputError


```csharp
OutputError = 5
```


The call failed. Maps to `'output-error'`.

**Value:** [ToolUIPartState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L271)
