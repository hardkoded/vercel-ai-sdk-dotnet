---
title: "MessagePruning"
description: "Message pruning. Maps to pruneMessages."
editUrl: false
prev: false
next: false
sidebar:
  label: "MessagePruning"
  hidden: true
---

<a id="Vercel_AI_GenerateText_MessagePruning"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class MessagePruning
```


Message pruning. Maps to `pruneMessages`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L89)

## Methods

<a id="Vercel_AI_GenerateText_MessagePruning_PruneMessages_"></a>
<a id="Vercel_AI_GenerateText_MessagePruning_PruneMessages_Vercel_AI_GenerateText_PruneMessagesOptions_"></a>

### PruneMessages(PruneMessagesOptions)


```csharp
public static IReadOnlyList<ModelMessage> PruneMessages(PruneMessagesOptions options)
```


Prunes reasoning, tool calls, results, errors, and approvals from a list of messages. The input is not changed.
A tool message upstream holds several parts. Here its parts are separate consecutive tool messages, and
they count as one message for [BeforeLastMessage](/api/Vercel.AI.GenerateText/PruneReasoning/#Vercel_AI_GenerateText_PruneReasoning_BeforeLastMessage) and the trailing-message rules.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [PruneMessagesOptions](/api/Vercel.AI.GenerateText/PruneMessagesOptions/) | The messages and the pruning settings. |

**Returns:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) — The pruned messages.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L129)
