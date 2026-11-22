---
title: "AmazonBedrockTools"
description: "Builds a Converse toolConfig from V4 function tools."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockTools"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockTools
```


Builds a Converse `toolConfig` from V4 function tools.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockTools.cs#L24)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools_Prepare_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools_Prepare_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_System_Collections_Generic_List_Vercel_AI_Provider_CallWarning___"></a>

### `Prepare(string, IReadOnlyList<LanguageModelTool>?, ToolChoice?, out List<CallWarning>)`


```csharp
public static JsonObject Prepare(string modelId, IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, out List<CallWarning> warnings)
```


Prepares tools. An empty or missing list returns an empty object.
Tool choice `none` clears the tools. A named choice keeps only that tool, except for
Anthropic models, which keep every function tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `warnings` | [List&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockTools.cs#L31)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools_Prepare_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_System_Nullable_System_Int32__System_Nullable_System_Boolean__System_Collections_Generic_List_Vercel_AI_Provider_CallWarning___System_Text_Json_Nodes_JsonObject__"></a>

### `Prepare(string, IReadOnlyList<LanguageModelTool>?, ToolChoice?, int?, bool?, out List<CallWarning>, out JsonObject?)`


```csharp
public static JsonObject Prepare(string modelId, IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, int? reasoningBudgetTokens, bool? disableParallelToolUse, out List<CallWarning> warnings, out JsonObject? additionalTools)
```


Prepares tools and the Anthropic `tool_choice` that goes into
`additionalModelRequestFields` (&lt;code class="paramref"&gt;additionalTools&lt;/code&gt;).
A named tool choice keeps every function tool for Anthropic models, so parallel tool use is
disabled by default to keep the response to the named tool.
`disableParallelToolUse: false` opts out.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `reasoningBudgetTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `disableParallelToolUse` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `warnings` | [List&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1) |  |
| `additionalTools` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockTools.cs#L45)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools_Prepare_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_System_Nullable_Vercel_AI_AmazonBedrock_AmazonBedrockChatModelFamily__System_Nullable_System_Int32__System_Nullable_System_Boolean__System_Boolean_System_Collections_Generic_List_Vercel_AI_Provider_CallWarning___System_Text_Json_Nodes_JsonObject__System_Collections_Generic_HashSet_System_String___"></a>

### `Prepare(string, IReadOnlyList<LanguageModelTool>?, ToolChoice?, AmazonBedrockChatModelFamily?, int?, bool?, bool, out List<CallWarning>, out JsonObject?, out HashSet<string>)`


```csharp
public static JsonObject Prepare(string modelId, IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, AmazonBedrockChatModelFamily? modelFamily, int? reasoningBudgetTokens, bool? disableParallelToolUse, bool rejectsForcedToolUse, out List<CallWarning> warnings, out JsonObject? additionalTools, out HashSet<string> betas)
```


Prepares function tools and Anthropic provider tools.
&lt;code class="paramref"&gt;betas&lt;/code&gt; are the Anthropic beta headers the provider tools need.
&lt;code class="paramref"&gt;rejectsForcedToolUse&lt;/code&gt; makes a model that rejects forced tool use fall back to
`auto` for tool choice `required` and `tool`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `modelFamily` | `AmazonBedrockChatModelFamily?` |  |
| `reasoningBudgetTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `disableParallelToolUse` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsForcedToolUse` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `warnings` | [List&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1) |  |
| `additionalTools` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `betas` | [HashSet&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockTools.cs#L73)
