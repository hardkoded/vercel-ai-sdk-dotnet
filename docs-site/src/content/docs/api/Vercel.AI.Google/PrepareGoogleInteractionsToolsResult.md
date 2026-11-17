---
title: "PrepareGoogleInteractionsToolsResult"
description: "Result of ToolChoice): the tools and toolchoice request fields and the warnings."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareGoogleInteractionsToolsResult"
  hidden: true
---

<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class PrepareGoogleInteractionsToolsResult
```


Result of `ToolChoice)`: the `tools` and `tool_choice` request fields and the warnings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L12)

## Constructors

<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult__ctor_"></a>
<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult__ctor_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonNode_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__"></a>

### `PrepareGoogleInteractionsToolsResult(JsonArray?, JsonNode?, IReadOnlyList<CallWarning>)`


```csharp
public PrepareGoogleInteractionsToolsResult(JsonArray? tools, JsonNode? toolChoice, IReadOnlyList<CallWarning> toolWarnings)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `toolChoice` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `toolWarnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L15)

## Properties

<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult_ToolChoice_"></a>
<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult_ToolChoice"></a>

### ToolChoice


```csharp
public JsonNode? ToolChoice { get; }
```


The Interactions `tool_choice`, or null when it is omitted.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L30)

<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult_ToolWarnings_"></a>
<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult_ToolWarnings"></a>

### ToolWarnings


```csharp
public IReadOnlyList<CallWarning> ToolWarnings { get; }
```


Warnings of dropped tools.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L33)

<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult_Tools_"></a>
<a id="Vercel_AI_Google_PrepareGoogleInteractionsToolsResult_Tools"></a>

### Tools


```csharp
public JsonArray? Tools { get; }
```


The Interactions `tools[]`, or null when there is none.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L27)
