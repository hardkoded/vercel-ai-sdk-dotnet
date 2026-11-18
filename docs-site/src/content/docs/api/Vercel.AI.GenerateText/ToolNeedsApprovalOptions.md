---
title: "ToolNeedsApprovalOptions"
description: "The options a tool's NeedsApprovalFunction receives. Maps to the second argument of the needsApproval function."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolNeedsApprovalOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolNeedsApprovalOptions
```


The options a tool's [NeedsApprovalFunction](/api/Vercel.AI/Tool/#Vercel_AI_Tool_NeedsApprovalFunction) receives. Maps to the second argument of the `needsApproval` function.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L11)

## Constructors

<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Nullable_System_Text_Json_JsonElement__"></a>

### `ToolNeedsApprovalOptions(string, IReadOnlyList<ModelMessage>, JsonElement?)`


```csharp
public ToolNeedsApprovalOptions(string toolCallId, IReadOnlyList<ModelMessage> messages, JsonElement? context = null)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `context` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L14)

## Properties

<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions_Context_"></a>
<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions_Context"></a>

### Context


```csharp
public JsonElement? Context { get; }
```


The context of the tool, validated with its [ContextSchema](/api/Vercel.AI/Tool/#Vercel_AI_Tool_ContextSchema). Null when none was given.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L26)

<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions_Messages_"></a>
<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


Messages that were sent to the language model to initiate the response that contained the tool call.
They do not include the instructions nor the assistant response that contained the tool call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L35)

<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_ToolNeedsApprovalOptions_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L29)
