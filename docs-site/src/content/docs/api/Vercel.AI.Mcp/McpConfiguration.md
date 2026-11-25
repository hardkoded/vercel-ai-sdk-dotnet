---
title: "McpConfiguration"
description: "The name, version and optional title of an MCP client or server. Maps to upstream's Configuration."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpConfiguration"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpConfiguration"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpConfiguration
```


The name, version and optional title of an MCP client or server. Maps to upstream's `Configuration`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L72)

## Constructors

<a id="Vercel_AI_Mcp_McpConfiguration__ctor_"></a>
<a id="Vercel_AI_Mcp_McpConfiguration__ctor_System_String_System_String_System_String_"></a>

### McpConfiguration(string, string, string?)


```csharp
public McpConfiguration(string name, string version, string? title = null)
```


Creates a configuration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `version` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L75)

## Properties

<a id="Vercel_AI_Mcp_McpConfiguration_Name_"></a>
<a id="Vercel_AI_Mcp_McpConfiguration_Name"></a>

### Name


```csharp
public string Name { get; }
```


The name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L83)

<a id="Vercel_AI_Mcp_McpConfiguration_Title_"></a>
<a id="Vercel_AI_Mcp_McpConfiguration_Title"></a>

### Title


```csharp
public string? Title { get; }
```


The human-readable title, if the server sent one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L89)

<a id="Vercel_AI_Mcp_McpConfiguration_Version_"></a>
<a id="Vercel_AI_Mcp_McpConfiguration_Version"></a>

### Version


```csharp
public string Version { get; }
```


The version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L86)
