---
title: "OAuthResources"
description: "OAuth resource URL checks from the MCP authorization helper."
editUrl: false
prev: false
next: false
sidebar:
  label: "OAuthResources"
  hidden: true
---

<a id="Vercel_AI_Mcp_OAuthResources"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class OAuthResources
```


OAuth resource URL checks from the MCP authorization helper.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L719)

## Methods

<a id="Vercel_AI_Mcp_OAuthResources_CheckResourceAllowed_"></a>
<a id="Vercel_AI_Mcp_OAuthResources_CheckResourceAllowed_System_String_System_String_"></a>

### CheckResourceAllowed(string, string)


```csharp
public static bool CheckResourceAllowed(string requestedResource, string configuredResource)
```


True when the requested resource has the same origin and its path is under the configured path.
A path matches itself and its children, not a different prefix such as `/mcpxxxx` for `/mcp`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `requestedResource` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `configuredResource` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L736)

<a id="Vercel_AI_Mcp_OAuthResources_ResourceUrlFromServerUrl_"></a>
<a id="Vercel_AI_Mcp_OAuthResources_ResourceUrlFromServerUrl_System_String_"></a>

### ResourceUrlFromServerUrl(string)


```csharp
public static string ResourceUrlFromServerUrl(string url)
```


Removes the fragment. The result uses the absolute URI form.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L722)
