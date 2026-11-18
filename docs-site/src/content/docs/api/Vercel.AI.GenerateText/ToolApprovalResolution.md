---
title: "ToolApprovalResolution"
description: "Resolves the approval state of a tool call. Maps to resolveToolApproval."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalResolution"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalResolution"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolApprovalResolution
```


Resolves the approval state of a tool call. Maps to `resolveToolApproval`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L39)

## Methods

<a id="Vercel_AI_GenerateText_ToolApprovalResolution_ResolveToolApprovalAsync_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalResolution_ResolveToolApprovalAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Tool__Vercel_AI_Provider_GeneratedToolCall_Vercel_AI_GenerateText_ToolApprovalConfiguration_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `ResolveToolApprovalAsync(IReadOnlyList<Tool>?, GeneratedToolCall, ToolApprovalConfiguration?, IReadOnlyList<ModelMessage>, IReadOnlyDictionary<string, JsonElement>?, IReadOnlyDictionary<string, object?>?)`


```csharp
public static Task<ToolApprovalStatus> ResolveToolApprovalAsync(IReadOnlyList<Tool>? tools, GeneratedToolCall toolCall, ToolApprovalConfiguration? toolApproval, IReadOnlyList<ModelMessage> messages, IReadOnlyDictionary<string, JsonElement>? toolsContext = null, IReadOnlyDictionary<string, object?>? runtimeContext = null)
```


Resolves the approval state for a tool call by checking the user-supplied and the tool-defined approval settings.
The user-supplied settings take precedence. Without any setting, the call does not require approval.
The result is never null: a null status becomes `not-applicable`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Tools that are available for the model to call. |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) | A valid tool call. |
| `toolApproval` | [ToolApprovalConfiguration](/api/Vercel.AI.GenerateText/ToolApprovalConfiguration/) | User-defined approval configuration. It takes precedence over the tool-defined settings. |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Messages that were sent to the language model to initiate the response that contained the tool call. |
| `toolsContext` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | The context of each tool, keyed by tool name. |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | User-defined runtime context. |

**Returns:** [Task&lt;ToolApprovalStatus&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [TypeValidationException](/api/Vercel.AI.GenerateText/TypeValidationException/) | The context of the tool does not match its [ContextSchema](/api/Vercel.AI/Tool/#Vercel_AI_Tool_ContextSchema). |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ResolveToolApproval.cs#L53)
