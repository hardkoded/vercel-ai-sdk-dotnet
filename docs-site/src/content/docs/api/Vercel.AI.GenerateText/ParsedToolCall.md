---
title: "ParsedToolCall"
description: "A tool call after its input was parsed and validated. Maps to a TypedToolCall."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParsedToolCall"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ParsedToolCall"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ParsedToolCall
```


A tool call after its input was parsed and validated. Maps to a `TypedToolCall`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L13)

## Constructors

<a id="Vercel_AI_GenerateText_ParsedToolCall__ctor_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### ParsedToolCall(string, string, JsonElement)


```csharp
public ParsedToolCall(string toolCallId, string toolName, JsonElement input)
```


Creates a parsed tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L16)

## Properties

<a id="Vercel_AI_GenerateText_ParsedToolCall_Dynamic_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_Dynamic"></a>

### Dynamic


```csharp
public bool Dynamic { get; init; }
```


True when the tool is dynamic: a dynamic tool, a provider-executed dynamic call, or an invalid call.
Maps to `dynamic`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L39)

<a id="Vercel_AI_GenerateText_ParsedToolCall_Error_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_Error"></a>

### Error


```csharp
public Exception? Error { get; init; }
```


Why the call is invalid, such as an [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/), a [NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/)
or a [ToolCallRepairError](/api/Vercel.AI.Util/ToolCallRepairError/).

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L51)

<a id="Vercel_AI_GenerateText_ParsedToolCall_Input_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_Input"></a>

### Input


```csharp
public JsonElement Input { get; }
```


The parsed input. When [Invalid](#Vercel_AI_GenerateText_ParsedToolCall_Invalid) is true this is the raw input, parsed when it is valid JSON
and a JSON string otherwise.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L33)

<a id="Vercel_AI_GenerateText_ParsedToolCall_Invalid_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_Invalid"></a>

### Invalid


```csharp
public bool Invalid { get; init; }
```


True when the call failed: its tool is unknown, its input did not parse, or the input did not match the tool's
input schema. Such a call is dynamic.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L45)

<a id="Vercel_AI_GenerateText_ParsedToolCall_ProviderExecuted_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; init; }
```


True when the provider executed the tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L57)

<a id="Vercel_AI_GenerateText_ParsedToolCall_ProviderMetadata_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; init; }
```


Provider-specific metadata of the call, as the model returned it.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L60)

<a id="Vercel_AI_GenerateText_ParsedToolCall_Title_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_Title"></a>

### Title


```csharp
public string? Title { get; init; }
```


Display title of the tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L54)

<a id="Vercel_AI_GenerateText_ParsedToolCall_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Provider id for this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L24)

<a id="Vercel_AI_GenerateText_ParsedToolCall_ToolMetadata_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_ToolMetadata"></a>

### ToolMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }
```


Metadata of the tool. It stays apart from [ProviderMetadata](#Vercel_AI_GenerateText_ParsedToolCall_ProviderMetadata). Maps to `toolMetadata`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L63)

<a id="Vercel_AI_GenerateText_ParsedToolCall_ToolName_"></a>
<a id="Vercel_AI_GenerateText_ParsedToolCall_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L27)
