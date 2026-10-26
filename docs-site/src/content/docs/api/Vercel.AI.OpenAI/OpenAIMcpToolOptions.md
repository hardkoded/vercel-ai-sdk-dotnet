---
title: "OpenAIMcpToolOptions"
description: "Options of Mcp(OpenAIMcpToolOptions). One of ServerUrl or ConnectorId must be set."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIMcpToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIMcpToolOptions
```


Options of [Mcp(OpenAIMcpToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_Mcp_Vercel_AI_OpenAI_OpenAIMcpToolOptions_). One of [ServerUrl](#Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerUrl) or [ConnectorId](#Vercel_AI_OpenAI_OpenAIMcpToolOptions_ConnectorId) must be set.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L243)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_AllowedToolNames_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_AllowedToolNames"></a>

### AllowedToolNames


```csharp
public IReadOnlyList<string>? AllowedToolNames { get; set; }
```


Allowed tool names.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L249)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_AllowedToolsReadOnly_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_AllowedToolsReadOnly"></a>

### AllowedToolsReadOnly


```csharp
public bool? AllowedToolsReadOnly { get; set; }
```


Allow only read-only tools.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L252)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_Authorization_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_Authorization"></a>

### Authorization


```csharp
public string? Authorization { get; set; }
```


OAuth access token for the MCP server or connector.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L255)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ConnectorId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ConnectorId"></a>

### ConnectorId


```csharp
public string? ConnectorId { get; set; }
```


Identifier of a service connector.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L258)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


HTTP headers to send to the MCP server.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L261)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_NeverRequireApprovalToolNames_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_NeverRequireApprovalToolNames"></a>

### NeverRequireApprovalToolNames


```csharp
public IReadOnlyList<string>? NeverRequireApprovalToolNames { get; set; }
```


Tool names that never require approval.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L267)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_RequireApproval_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_RequireApproval"></a>

### RequireApproval


```csharp
public string? RequireApproval { get; set; }
```


Approval policy: `always` or `never`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L264)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerDescription_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerDescription"></a>

### ServerDescription


```csharp
public string? ServerDescription { get; set; }
```


Description of the server.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L270)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerLabel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerLabel"></a>

### ServerLabel


```csharp
public string ServerLabel { get; set; }
```


A label for this MCP server, used to identify it in tool calls.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L246)

<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerUrl_"></a>
<a id="Vercel_AI_OpenAI_OpenAIMcpToolOptions_ServerUrl"></a>

### ServerUrl


```csharp
public string? ServerUrl { get; set; }
```


URL of the MCP server.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L273)
