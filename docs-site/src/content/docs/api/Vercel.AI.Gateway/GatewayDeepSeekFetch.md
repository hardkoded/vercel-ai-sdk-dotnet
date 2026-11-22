---
title: "GatewayDeepSeekFetch"
description: "Declares the Gateway DeepSeek Fetch tool: fetches a page as Markdown through DeepSeek's standalone Fetch API. The AI Gateway executes it for any model, separ..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayDeepSeekFetch"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayDeepSeekFetch
```


Declares the Gateway DeepSeek Fetch tool: fetches a page as Markdown through DeepSeek's standalone Fetch API. The AI Gateway executes
it for any model, separate from native model web search. Maps to `gatewayTools.deepseekFetch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L15)

## Properties

<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch_InputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch_InputSchema"></a>

### InputSchema


```csharp
public static JsonElement InputSchema { get; }
```


Input schema the model fills: `url`.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L46)

<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch_OutputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch_OutputSchema"></a>

### OutputSchema


```csharp
public static JsonElement OutputSchema { get; }
```


Output schema: a fetch response or a [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/).

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L49)

## Methods

<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch_Create_"></a>
<a id="Vercel_AI_Gateway_GatewayDeepSeekFetch_Create_Vercel_AI_Gateway_GatewayDeepSeekFetchConfig_"></a>

### Create(GatewayDeepSeekFetchConfig?)


```csharp
public static Tool Create(GatewayDeepSeekFetchConfig? config = null)
```


Creates the DeepSeek Fetch tool. The tool is executed by the AI Gateway, so it has no local execute function.
Maps to `gatewayTools.deepseekFetch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [GatewayDeepSeekFetchConfig](/api/Vercel.AI.Gateway/GatewayDeepSeekFetchConfig/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDeepSeekFetch.cs#L55)
