---
title: "PruneToolCallsRule"
description: "One rule for pruning tool calls, results, errors, and approvals. Maps to an entry of the toolCalls array, { type: 'all' | 'before-last-message' | 'before-las..."
editUrl: false
prev: false
next: false
sidebar:
  label: "PruneToolCallsRule"
  hidden: true
---

<a id="Vercel_AI_GenerateText_PruneToolCallsRule"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class PruneToolCallsRule
```


One rule for pruning tool calls, results, errors, and approvals. Maps to an entry of the `toolCalls` array,
`{ type: 'all' | 'before-last-message' | 'before-last-N-messages', tools }`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L36)

## Properties

<a id="Vercel_AI_GenerateText_PruneToolCallsRule_KeepLastMessagesCount_"></a>
<a id="Vercel_AI_GenerateText_PruneToolCallsRule_KeepLastMessagesCount"></a>

### KeepLastMessagesCount


```csharp
public int? KeepLastMessagesCount { get; }
```


Number of trailing messages to keep, or null for none (`all`).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L66)

<a id="Vercel_AI_GenerateText_PruneToolCallsRule_Tools_"></a>
<a id="Vercel_AI_GenerateText_PruneToolCallsRule_Tools"></a>

### Tools


```csharp
public IReadOnlyList<string>? Tools { get; }
```


Names of the tools to prune, or null for every tool.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L69)

## Methods

<a id="Vercel_AI_GenerateText_PruneToolCallsRule_All_"></a>
<a id="Vercel_AI_GenerateText_PruneToolCallsRule_All_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `All(IReadOnlyList<string>?)`


```csharp
public static PruneToolCallsRule All(IReadOnlyList<string>? tools = null)
```


Prunes in all messages. Maps to `type: 'all'`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Names of the tools to prune. Null prunes every tool. |

**Returns:** [PruneToolCallsRule](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L46)

<a id="Vercel_AI_GenerateText_PruneToolCallsRule_BeforeLastMessage_"></a>
<a id="Vercel_AI_GenerateText_PruneToolCallsRule_BeforeLastMessage_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `BeforeLastMessage(IReadOnlyList<string>?)`


```csharp
public static PruneToolCallsRule BeforeLastMessage(IReadOnlyList<string>? tools = null)
```


Prunes before the last message. Maps to `type: 'before-last-message'`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Names of the tools to prune. Null prunes every tool. |

**Returns:** [PruneToolCallsRule](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L50)

<a id="Vercel_AI_GenerateText_PruneToolCallsRule_BeforeLastMessages_"></a>
<a id="Vercel_AI_GenerateText_PruneToolCallsRule_BeforeLastMessages_System_Int32_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `BeforeLastMessages(int, IReadOnlyList<string>?)`


```csharp
public static PruneToolCallsRule BeforeLastMessages(int count, IReadOnlyList<string>? tools = null)
```


Prunes before the last &lt;code class="paramref"&gt;count&lt;/code&gt; messages. Maps to `type: 'before-last-N-messages'`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `count` | [int](https://learn.microsoft.com/dotnet/api/system.int32) | Number of trailing messages to keep unpruned. |
| `tools` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Names of the tools to prune. Null prunes every tool. |

**Returns:** [PruneToolCallsRule](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/PruneMessages.cs#L55)
