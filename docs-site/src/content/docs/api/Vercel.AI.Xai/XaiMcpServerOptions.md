---
title: "XaiMcpServerOptions"
description: "Arguments of String). Maps to mcpServerArgsSchema."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiMcpServerOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiMcpServerOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiMcpServerOptions
```


Arguments of `String)`. Maps to `mcpServerArgsSchema`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L85)

## Constructors

<a id="Vercel_AI_Xai_XaiMcpServerOptions__ctor_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions__ctor_System_String_"></a>

### XaiMcpServerOptions(string)


```csharp
public XaiMcpServerOptions(string serverUrl)
```


Creates the arguments.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serverUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L88)

## Properties

<a id="Vercel_AI_Xai_XaiMcpServerOptions_AllowedTools_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions_AllowedTools"></a>

### AllowedTools


```csharp
public IReadOnlyList<string>? AllowedTools { get; init; }
```


Names of the tools the model may call.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L103)

<a id="Vercel_AI_Xai_XaiMcpServerOptions_Authorization_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions_Authorization"></a>

### Authorization


```csharp
public string? Authorization { get; init; }
```


The authorization header value.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L109)

<a id="Vercel_AI_Xai_XaiMcpServerOptions_Headers_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; init; }
```


Custom headers to send.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L106)

<a id="Vercel_AI_Xai_XaiMcpServerOptions_ServerDescription_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions_ServerDescription"></a>

### ServerDescription


```csharp
public string? ServerDescription { get; init; }
```


A description of the MCP server.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L100)

<a id="Vercel_AI_Xai_XaiMcpServerOptions_ServerLabel_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions_ServerLabel"></a>

### ServerLabel


```csharp
public string? ServerLabel { get; init; }
```


A label for the MCP server.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L97)

<a id="Vercel_AI_Xai_XaiMcpServerOptions_ServerUrl_"></a>
<a id="Vercel_AI_Xai_XaiMcpServerOptions_ServerUrl"></a>

### ServerUrl


```csharp
public string ServerUrl { get; }
```


The URL of the MCP server.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L94)
