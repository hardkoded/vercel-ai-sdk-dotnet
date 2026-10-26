---
title: "PreparedResponsesTools"
description: "Tools prepared for the OpenAI Responses API."
editUrl: false
prev: false
next: false
sidebar:
  label: "PreparedResponsesTools"
  hidden: true
---

<a id="Vercel_AI_OpenAI_PreparedResponsesTools"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class PreparedResponsesTools
```


Tools prepared for the OpenAI Responses API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L68)

## Constructors

<a id="Vercel_AI_OpenAI_PreparedResponsesTools__ctor_"></a>
<a id="Vercel_AI_OpenAI_PreparedResponsesTools__ctor_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonNode_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__"></a>

### `PreparedResponsesTools(JsonArray?, JsonNode?, IReadOnlyList<CallWarning>)`


```csharp
public PreparedResponsesTools(JsonArray? tools, JsonNode? toolChoice, IReadOnlyList<CallWarning> toolWarnings)
```


Creates a prepared tool list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `toolChoice` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `toolWarnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L71)

## Properties

<a id="Vercel_AI_OpenAI_PreparedResponsesTools_ToolChoice_"></a>
<a id="Vercel_AI_OpenAI_PreparedResponsesTools_ToolChoice"></a>

### ToolChoice


```csharp
public JsonNode? ToolChoice { get; }
```


The `tool_choice` request value. A named choice maps to the matching hosted tool, a custom tool, or a function tool. Null when unset.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L84)

<a id="Vercel_AI_OpenAI_PreparedResponsesTools_ToolWarnings_"></a>
<a id="Vercel_AI_OpenAI_PreparedResponsesTools_ToolWarnings"></a>

### ToolWarnings


```csharp
public IReadOnlyList<CallWarning> ToolWarnings { get; }
```


Warnings produced while preparing tools.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L87)

<a id="Vercel_AI_OpenAI_PreparedResponsesTools_Tools_"></a>
<a id="Vercel_AI_OpenAI_PreparedResponsesTools_Tools"></a>

### Tools


```csharp
public JsonArray? Tools { get; }
```


Provider tool objects. Null when the request has no tools.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L79)
