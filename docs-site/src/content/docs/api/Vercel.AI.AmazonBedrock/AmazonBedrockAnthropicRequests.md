---
title: "AmazonBedrockAnthropicRequests"
description: "Builds InvokeModel URLs and the Bedrock Anthropic request body."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockAnthropicRequests"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockAnthropicRequests
```


Builds InvokeModel URLs and the Bedrock Anthropic request body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L64)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests_BuildUrl_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests_BuildUrl_System_String_System_String_System_Boolean_"></a>

### BuildUrl(string, string, bool)


```csharp
public static string BuildUrl(string baseUrl, string modelId, bool streaming)
```


InvokeModel URL. Streaming uses `invoke-with-response-stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `streaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L67)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests_ToAnthropicErrorBody_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests_ToAnthropicErrorBody_System_String_"></a>

### ToAnthropicErrorBody(string)


```csharp
public static string ToAnthropicErrorBody(string text)
```


Rewrites a Bedrock error body into the Anthropic `{ type: error, error }` shape.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L226)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests_Transform_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicRequests_Transform_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `Transform(JsonObject, IReadOnlyList<string>?)`


```csharp
public static JsonObject Transform(JsonObject source, IReadOnlyList<string>? betas = null)
```


Drops `model` and `stream`, keeps a tool choice type and name, renames the thinking
block binding field, upgrades computer-use tool versions, moves the betas into
`anthropic_beta`, and sets `anthropic_version` to `bedrock-2023-05-31`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `betas` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L104)
