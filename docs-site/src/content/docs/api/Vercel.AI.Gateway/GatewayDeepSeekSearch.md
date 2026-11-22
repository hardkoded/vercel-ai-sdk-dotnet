---
title: "GatewayDeepSeekSearch"
description: "Declares the Gateway DeepSeek Search tool: searches the web through DeepSeek's standalone Search API. The AI Gateway executes it for any model, separate from..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayDeepSeekSearch"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayDeepSeekSearch
```


Declares the Gateway DeepSeek Search tool: searches the web through DeepSeek's standalone Search API. The AI Gateway executes it for
any model, separate from native model web search. Maps to `gatewayTools.deepseekSearch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L15)

## Properties

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch_InputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch_InputSchema"></a>

### InputSchema


```csharp
public static JsonElement InputSchema { get; }
```


Input schema the model fills: `query`, `count`, `need_content` and `ori_question`. `localization` is developer-only.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L62)

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch_OutputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch_OutputSchema"></a>

### OutputSchema


```csharp
public static JsonElement OutputSchema { get; }
```


Output schema: a search response or a [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/).

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L65)

## Methods

<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch_Create_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekSearch_Create_Vercel_AI_Gateway_GatewayDeepSeekSearchConfig_"></a>

### Create(GatewayDeepSeekSearchConfig?)


```csharp
public static Tool Create(GatewayDeepSeekSearchConfig? config = null)
```


Creates the DeepSeek Search tool. The tool is executed by the AI Gateway, so it has no local execute function.
Maps to `gatewayTools.deepseekSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [GatewayDeepSeekSearchConfig](/api/Vercel.AI.Gateway/GatewayDeepSeekSearchConfig/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekSearch.cs#L71)
