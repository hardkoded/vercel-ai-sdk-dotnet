---
title: "JsonRpcMessages"
description: "Checks JSON-RPC 2.0 requests, notifications, results, and errors."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonRpcMessages"
  hidden: true
---

<a id="Vercel_AI_Mcp_JsonRpcMessages"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class JsonRpcMessages
```


Checks JSON-RPC 2.0 requests, notifications, results, and errors.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L15)

## Methods

<a id="Vercel_AI_Mcp_JsonRpcMessages_Parse_"></a>
<a id="Vercel_AI_Mcp_JsonRpcMessages_Parse_System_String_"></a>

### Parse(string)


```csharp
public static JsonElement Parse(string text)
```


Parses one JSON-RPC message. Prototype-pollution keys are rejected.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L18)

<a id="Vercel_AI_Mcp_JsonRpcMessages_Validate_"></a>
<a id="Vercel_AI_Mcp_JsonRpcMessages_Validate_System_Text_Json_JsonElement_"></a>

### Validate(JsonElement)


```csharp
public static void Validate(JsonElement message)
```


Throws [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) when &lt;code class="paramref"&gt;message&lt;/code&gt; is not a JSON-RPC 2.0 message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L26)
