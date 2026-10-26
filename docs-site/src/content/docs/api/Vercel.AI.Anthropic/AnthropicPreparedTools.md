---
title: "AnthropicPreparedTools"
description: "Tools and tool choice ready for a Messages request."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicPreparedTools"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicPreparedTools"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicPreparedTools
```


Tools and tool choice ready for a Messages request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L42)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicPreparedTools__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedTools__ctor_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicWarning__System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `AnthropicPreparedTools(JsonArray?, JsonObject?, IReadOnlyList<AnthropicWarning>, IReadOnlyList<string>)`


```csharp
public AnthropicPreparedTools(JsonArray? tools, JsonObject? toolChoice, IReadOnlyList<AnthropicWarning> warnings, IReadOnlyList<string> betas)
```


Creates a prepared tool set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `toolChoice` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L45)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_Betas_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_Betas"></a>

### Betas


```csharp
public IReadOnlyList<string> Betas { get; }
```


Beta headers required by the tools.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L63)

<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_ToolChoice_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_ToolChoice"></a>

### ToolChoice


```csharp
public JsonObject? ToolChoice { get; }
```


Anthropic `tool_choice`. Null when omitted.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L57)

<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_Tools_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_Tools"></a>

### Tools


```csharp
public JsonArray? Tools { get; }
```


Anthropic `tools` array. Null when tools are omitted.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L54)

<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicPreparedTools_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<AnthropicWarning> Warnings { get; }
```


Warnings from unsupported tools or strict mode.

**Value:** [IReadOnlyList&lt;AnthropicWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L60)
