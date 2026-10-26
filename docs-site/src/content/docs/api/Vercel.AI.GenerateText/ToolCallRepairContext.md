---
title: "ToolCallRepairContext"
description: "Context passed to a ToolCallRepairFunction. Maps to the options of ToolCallRepairFunction."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallRepairContext"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolCallRepairContext"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolCallRepairContext
```


Context passed to a [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/). Maps to the options of `ToolCallRepairFunction`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L74)

## Constructors

<a id="Vercel_AI_GenerateText_ToolCallRepairContext__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext__ctor_Vercel_AI_Provider_GeneratedToolCall_System_Collections_Generic_IReadOnlyList_Vercel_AI_Tool__System_Func_System_String_System_Threading_Tasks_Task_System_Text_Json_JsonElement___Vercel_AI_Util_AiSdkError_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__Vercel_AI_Prompt_Instructions_System_Threading_CancellationToken_"></a>

### `ToolCallRepairContext(GeneratedToolCall, IReadOnlyList<Tool>, Func<string, Task<JsonElement>>, AiSdkError, IReadOnlyList<ModelMessage>, Instructions?, CancellationToken)`


```csharp
public ToolCallRepairContext(GeneratedToolCall toolCall, IReadOnlyList<Tool> tools, Func<string, Task<JsonElement>> inputSchema, AiSdkError error, IReadOnlyList<ModelMessage> messages, Instructions? instructions, CancellationToken cancellationToken)
```


Creates a repair context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `tools` | [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `inputSchema` | [Func&lt;string, Task&lt;JsonElement&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `error` | [AiSdkError](/api/Vercel.AI.Util/AiSdkError/) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `instructions` | [Instructions](/api/Vercel.AI.Prompt/Instructions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L77)

## Properties

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_CancellationToken_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancels the repair. Maps to `abortSignal`.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L117)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Error_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Error"></a>

### Error


```csharp
public AiSdkError Error { get; }
```


The [NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/) or [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/) raised while parsing the call.

**Value:** [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L105)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_InputSchema_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_InputSchema"></a>

### InputSchema


```csharp
public Func<string, Task<JsonElement>> InputSchema { get; }
```


Returns the JSON Schema of the input of the named tool.

**Value:** [Func&lt;string, Task&lt;JsonElement&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L102)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Instructions_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; }
```


The instructions given to the model.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L111)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Messages_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


The messages of the current generation step.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L108)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_System_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_System"></a>

### System


```csharp
public Instructions? System { get; }
```


Same as [Instructions](#Vercel_AI_GenerateText_ToolCallRepairContext_Instructions). Deprecated upstream; use [Instructions](#Vercel_AI_GenerateText_ToolCallRepairContext_Instructions).

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L114)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_ToolCall_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The tool call that failed to parse.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L96)

<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Tools_"></a>
<a id="Vercel_AI_GenerateText_ToolCallRepairContext_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool> Tools { get; }
```


The tools that are available.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L99)
