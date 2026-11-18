---
title: "GenericToolApprovalOptions"
description: "The options a GenericToolApprovalFunction receives. Maps to the options of GenericToolApprovalFunction."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenericToolApprovalOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class GenericToolApprovalOptions
```


The options a [GenericToolApprovalFunction](/api/Vercel.AI.GenerateText/GenericToolApprovalFunction/) receives. Maps to the options of `GenericToolApprovalFunction`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L122)

## Constructors

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions__ctor_"></a>
<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions__ctor_Vercel_AI_Provider_GeneratedToolCall_System_Collections_Generic_IReadOnlyList_Vercel_AI_Tool__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `GenericToolApprovalOptions(GeneratedToolCall, IReadOnlyList<Tool>?, IReadOnlyList<ModelMessage>, IReadOnlyDictionary<string, JsonElement>?, IReadOnlyDictionary<string, object?>?)`


```csharp
public GenericToolApprovalOptions(GeneratedToolCall toolCall, IReadOnlyList<Tool>? tools, IReadOnlyList<ModelMessage> messages, IReadOnlyDictionary<string, JsonElement>? toolsContext = null, IReadOnlyDictionary<string, object?>? runtimeContext = null)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `tools` | [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolsContext` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L125)

## Properties

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_Messages_"></a>
<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


Messages that were sent to the language model to initiate the response that contained the tool call.
They do not include the instructions nor the assistant response that contained the tool call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L156)

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_RuntimeContext_"></a>
<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; }
```


The user-defined runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L144)

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_ToolCall_"></a>
<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The tool call that needs approval.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L147)

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_Tools_"></a>
<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; }
```


All tools that are available for the model to call.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L150)

<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_ToolsContext_"></a>
<a id="Vercel_AI_GenerateText_GenericToolApprovalOptions_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; }
```


The context of each tool, keyed by tool name, as given. It is not validated.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L141)
