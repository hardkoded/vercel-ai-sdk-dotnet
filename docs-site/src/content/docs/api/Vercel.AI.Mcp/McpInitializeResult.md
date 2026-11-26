---
title: "McpInitializeResult"
description: "The result of the initialize request, or its server/discover equivalent. Maps to upstream's InitializeResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpInitializeResult"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpInitializeResult"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpInitializeResult
```


The result of the `initialize` request, or its `server/discover` equivalent. Maps to upstream's `InitializeResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L124)

## Constructors

<a id="Vercel_AI_Mcp_McpInitializeResult__ctor_"></a>
<a id="Vercel_AI_Mcp_McpInitializeResult__ctor_System_String_System_Text_Json_JsonElement_Vercel_AI_Mcp_McpConfiguration_System_String_"></a>

### McpInitializeResult(string, JsonElement, McpConfiguration, string?)


```csharp
public McpInitializeResult(string protocolVersion, JsonElement capabilities, McpConfiguration serverInfo, string? instructions = null)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `protocolVersion` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `capabilities` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `serverInfo` | [McpConfiguration](/api/Vercel.AI.Mcp/McpConfiguration/) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L127)

## Properties

<a id="Vercel_AI_Mcp_McpInitializeResult_Capabilities_"></a>
<a id="Vercel_AI_Mcp_McpInitializeResult_Capabilities"></a>

### Capabilities


```csharp
public JsonElement Capabilities { get; }
```


The server capabilities.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L144)

<a id="Vercel_AI_Mcp_McpInitializeResult_Instructions_"></a>
<a id="Vercel_AI_Mcp_McpInitializeResult_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; }
```


The server instructions, if any.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L150)

<a id="Vercel_AI_Mcp_McpInitializeResult_ProtocolVersion_"></a>
<a id="Vercel_AI_Mcp_McpInitializeResult_ProtocolVersion"></a>

### ProtocolVersion


```csharp
public string ProtocolVersion { get; }
```


The negotiated protocol version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L141)

<a id="Vercel_AI_Mcp_McpInitializeResult_ServerInfo_"></a>
<a id="Vercel_AI_Mcp_McpInitializeResult_ServerInfo"></a>

### ServerInfo


```csharp
public McpConfiguration ServerInfo { get; }
```


The server name and version.

**Value:** [McpConfiguration](/api/Vercel.AI.Mcp/McpConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L147)
