---
title: "AmazonBedrockMessages"
description: "Converts V4 prompt messages into Converse system and messages arrays."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockMessages"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockMessages"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockMessages
```


Converts V4 prompt messages into Converse `system` and `messages` arrays.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockMessages.cs#L13)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockMessages_Convert_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockMessages_Convert_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Text_Json_Nodes_JsonArray__System_Text_Json_Nodes_JsonArray__"></a>

### `Convert(string, IReadOnlyList<ModelMessage>, out JsonArray?, out JsonArray)`


```csharp
public static void Convert(string modelId, IReadOnlyList<ModelMessage> prompt, out JsonArray? system, out JsonArray messages)
```


Groups leading system messages, merges adjacent user and tool messages, and emits assistant tool uses.
A system message after another role throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `system` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `messages` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockMessages.cs#L19)
