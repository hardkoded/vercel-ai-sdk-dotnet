---
title: "ToolExecutionOptions"
description: "The options a tool receives when it runs. Maps to the second argument of execute."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecutionOptions"
  hidden: true
---

<a id="Vercel_AI_ToolExecutionOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolExecutionOptions
```


The options a tool receives when it runs. Maps to the second argument of `execute`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L129)

## Constructors

<a id="Vercel_AI_ToolExecutionOptions__ctor_"></a>
<a id="Vercel_AI_ToolExecutionOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Threading_CancellationToken_"></a>

### `ToolExecutionOptions(string, IReadOnlyList<ModelMessage>, CancellationToken)`


```csharp
public ToolExecutionOptions(string toolCallId, IReadOnlyList<ModelMessage> messages, CancellationToken abortSignal)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L132)

## Properties

<a id="Vercel_AI_ToolExecutionOptions_AbortSignal_"></a>
<a id="Vercel_AI_ToolExecutionOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; }
```


Cancellation signal of the call.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L146)

<a id="Vercel_AI_ToolExecutionOptions_Messages_"></a>
<a id="Vercel_AI_ToolExecutionOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


Messages sent to the model in the step that made the call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L143)

<a id="Vercel_AI_ToolExecutionOptions_ToolCallId_"></a>
<a id="Vercel_AI_ToolExecutionOptions_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L140)
