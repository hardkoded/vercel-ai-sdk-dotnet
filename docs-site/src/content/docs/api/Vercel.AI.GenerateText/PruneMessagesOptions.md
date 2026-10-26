---
title: "PruneMessagesOptions"
description: "Options of PruneMessages(PruneMessagesOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "PruneMessagesOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_PruneMessagesOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class PruneMessagesOptions
```


Options of [PruneMessages(PruneMessagesOptions)](/api/Vercel.AI.GenerateText/MessagePruning/#Vercel_AI_GenerateText_MessagePruning_PruneMessages_Vercel_AI_GenerateText_PruneMessagesOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L73)

## Properties

<a id="Vercel_AI_GenerateText_PruneMessagesOptions_EmptyMessages_"></a>
<a id="Vercel_AI_GenerateText_PruneMessagesOptions_EmptyMessages"></a>

### EmptyMessages


```csharp
public PruneEmptyMessages EmptyMessages { get; set; }
```


Whether to keep or remove messages whose content is empty after pruning. Default is [Remove](/api/Vercel.AI.GenerateText/PruneEmptyMessages/#Vercel_AI_GenerateText_PruneEmptyMessages_Remove).

**Value:** [PruneEmptyMessages](/api/Vercel.AI.GenerateText/PruneEmptyMessages/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L85)

<a id="Vercel_AI_GenerateText_PruneMessagesOptions_Messages_"></a>
<a id="Vercel_AI_GenerateText_PruneMessagesOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; set; }
```


The messages to prune.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L76)

<a id="Vercel_AI_GenerateText_PruneMessagesOptions_Reasoning_"></a>
<a id="Vercel_AI_GenerateText_PruneMessagesOptions_Reasoning"></a>

### Reasoning


```csharp
public PruneReasoning Reasoning { get; set; }
```


How to remove reasoning from assistant messages. Default is [None](/api/Vercel.AI.GenerateText/PruneReasoning/#Vercel_AI_GenerateText_PruneReasoning_None).

**Value:** [PruneReasoning](/api/Vercel.AI.GenerateText/PruneReasoning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L79)

<a id="Vercel_AI_GenerateText_PruneMessagesOptions_ToolCalls_"></a>
<a id="Vercel_AI_GenerateText_PruneMessagesOptions_ToolCalls"></a>

### ToolCalls


```csharp
public IReadOnlyList<PruneToolCallsRule> ToolCalls { get; set; }
```


How to prune tool calls, results, errors, and approvals. Default is no rules.

**Value:** [IReadOnlyList&lt;PruneToolCallsRule&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L82)
