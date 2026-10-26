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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockTools.cs#L13)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools_Prepare_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockTools_Prepare_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_System_Collections_Generic_List_Vercel_AI_Provider_CallWarning___"></a>

### `Prepare(string, IReadOnlyList<LanguageModelTool>?, ToolChoice?, out List<CallWarning>)`


```csharp
public static JsonObject Prepare(string modelId, IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, out List<CallWarning> warnings)
```


Prepares function tools. An empty or missing list returns an empty object.
Tool choice `none` clears the tools. A named choice keeps only that tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `warnings` | [List&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockTools.cs#L27)
