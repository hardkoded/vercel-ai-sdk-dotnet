---
title: "GatewayTakoSearch"
description: "JSON schemas for the Gateway Tako search tool."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayTakoSearch"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayTakoSearch"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayTakoSearch
```


JSON schemas for the Gateway Tako search tool.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L12)

## Properties

<a id="Vercel_AI_Gateway_GatewayTakoSearch_InputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearch_InputSchema"></a>

### InputSchema


```csharp
public static JsonElement InputSchema { get; }
```


Input schema. Data-source descriptions name the surcharge controls.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L229)

<a id="Vercel_AI_Gateway_GatewayTakoSearch_OutputSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearch_OutputSchema"></a>

### OutputSchema


```csharp
public static JsonElement OutputSchema { get; }
```


Output schema. Internal ranking fields are not part of the contract.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L232)

<a id="Vercel_AI_Gateway_GatewayTakoSearch_ResultContentSchema_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearch_ResultContentSchema"></a>

### ResultContentSchema


```csharp
public static JsonElement ResultContentSchema { get; }
```


Schema of the `content` of a card or web result, exported for reuse by other Tako tools. Maps to `takoResultContentSchema`.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L235)

## Methods

<a id="Vercel_AI_Gateway_GatewayTakoSearch_Create_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearch_Create_Vercel_AI_Gateway_GatewayTakoSearchConfig_"></a>

### Create(GatewayTakoSearchConfig?)


```csharp
public static Tool Create(GatewayTakoSearchConfig? config = null)
```


Creates the Tako search tool. The tool is executed by the AI Gateway, so it has no local execute function.
Maps to `gatewayTools.takoSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [GatewayTakoSearchConfig](/api/Vercel.AI.Gateway/GatewayTakoSearchConfig/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L243)
