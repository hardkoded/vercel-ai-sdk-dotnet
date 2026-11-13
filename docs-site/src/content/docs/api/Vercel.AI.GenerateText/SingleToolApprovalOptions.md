---
title: "SingleToolApprovalOptions"
description: "The options a SingleToolApprovalFunction receives. Maps to the second argument of SingleToolApprovalFunction."
editUrl: false
prev: false
next: false
sidebar:
  label: "SingleToolApprovalOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class SingleToolApprovalOptions
```


The options a [SingleToolApprovalFunction](/api/Vercel.AI.GenerateText/SingleToolApprovalFunction/) receives. Maps to the second argument of `SingleToolApprovalFunction`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L72)

## Constructors

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions__ctor_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `SingleToolApprovalOptions(string, IReadOnlyList<ModelMessage>)`


```csharp
public SingleToolApprovalOptions(string toolCallId, IReadOnlyList<ModelMessage> messages)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L75)

## Properties

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_Messages_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


Messages that were sent to the language model to initiate the response that contained the tool call.
They do not include the instructions nor the assistant response that contained the tool call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L88)

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L82)
