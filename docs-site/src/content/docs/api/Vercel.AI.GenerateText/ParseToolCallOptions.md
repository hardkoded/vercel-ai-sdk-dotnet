---
title: "ParseToolCallOptions"
description: "Options of CancellationToken). Maps to the options of parseToolCall."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParseToolCallOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ParseToolCallOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ParseToolCallOptions
```


Options of `CancellationToken)`. Maps to the options of `parseToolCall`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L127)

## Constructors

<a id="Vercel_AI_GenerateText_ParseToolCallOptions__ctor_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions__ctor_Vercel_AI_Provider_GeneratedToolCall_"></a>

### ParseToolCallOptions(GeneratedToolCall)


```csharp
public ParseToolCallOptions(GeneratedToolCall toolCall)
```


Creates options for &lt;code class="paramref"&gt;toolCall&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L130)

## Properties

<a id="Vercel_AI_GenerateText_ParseToolCallOptions_Instructions_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; init; }
```


The instructions given to the model. They are passed to [RepairToolCall](#Vercel_AI_GenerateText_ParseToolCallOptions_RepairToolCall).

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L154)

<a id="Vercel_AI_GenerateText_ParseToolCallOptions_Messages_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; init; }
```


The messages of the current step. They are passed to [RepairToolCall](#Vercel_AI_GenerateText_ParseToolCallOptions_RepairToolCall).

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L151)

<a id="Vercel_AI_GenerateText_ParseToolCallOptions_RefineToolInput_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; init; }
```


Refines the input of a valid call, keyed by tool name. A tool without an entry keeps its parsed input.
Maps to `refineToolInput`.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L148)

<a id="Vercel_AI_GenerateText_ParseToolCallOptions_RepairToolCall_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions_RepairToolCall"></a>

### RepairToolCall


```csharp
public ToolCallRepairFunction? RepairToolCall { get; init; }
```


Repairs a call whose tool is unknown or whose input is invalid. Maps to `repairToolCall`.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L142)

<a id="Vercel_AI_GenerateText_ParseToolCallOptions_ToolCall_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The call to parse.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L136)

<a id="Vercel_AI_GenerateText_ParseToolCallOptions_Tools_"></a>
<a id="Vercel_AI_GenerateText_ParseToolCallOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; init; }
```


The available tools. Null means no tools were set, which differs from an empty list in the [NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/) message.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L139)
