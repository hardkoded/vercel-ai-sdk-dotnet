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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L80)

## Constructors

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions__ctor_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `SingleToolApprovalOptions(string, IReadOnlyList<ModelMessage>, JsonElement?, IReadOnlyDictionary<string, object?>?)`


```csharp
public SingleToolApprovalOptions(string toolCallId, IReadOnlyList<ModelMessage> messages, JsonElement? toolContext = null, IReadOnlyDictionary<string, object?>? runtimeContext = null)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolContext` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L83)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L109)

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_RuntimeContext_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; }
```


The user-defined runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L100)

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L103)

<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_ToolContext_"></a>
<a id="Vercel_AI_GenerateText_SingleToolApprovalOptions_ToolContext"></a>

### ToolContext


```csharp
public JsonElement? ToolContext { get; }
```


The context of the tool, validated with its [ContextSchema](/api/Vercel.AI/Tool/#Vercel_AI_Tool_ContextSchema). Null when none was given.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L97)
