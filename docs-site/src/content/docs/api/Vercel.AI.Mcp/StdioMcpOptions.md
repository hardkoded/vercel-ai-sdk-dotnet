---
title: "StdioMcpOptions"
description: "How to start an MCP stdio server. Mirrors the upstream StdioConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "StdioMcpOptions"
  hidden: true
---

<a id="Vercel_AI_Mcp_StdioMcpOptions"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class StdioMcpOptions
```


How to start an MCP stdio server. Mirrors the upstream `StdioConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L174)

## Constructors

<a id="Vercel_AI_Mcp_StdioMcpOptions__ctor_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions__ctor_System_String_"></a>

### StdioMcpOptions(string)


```csharp
public StdioMcpOptions(string command)
```


Creates options for &lt;code class="paramref"&gt;command&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `command` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L177)

## Properties

<a id="Vercel_AI_Mcp_StdioMcpOptions_Arguments_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_Arguments"></a>

### Arguments


```csharp
public IReadOnlyList<string>? Arguments { get; set; }
```


Command arguments.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L186)

<a id="Vercel_AI_Mcp_StdioMcpOptions_Command_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_Command"></a>

### Command


```csharp
public string Command { get; }
```


Executable to start.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L183)

<a id="Vercel_AI_Mcp_StdioMcpOptions_Environment_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_Environment"></a>

### Environment


```csharp
public IReadOnlyDictionary<string, string>? Environment { get; set; }
```


Extra environment variables. The child gets these plus a small set of safe inherited variables.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L189)

<a id="Vercel_AI_Mcp_StdioMcpOptions_RedirectStandardError_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_RedirectStandardError"></a>

### RedirectStandardError


```csharp
public bool RedirectStandardError { get; set; }
```


True pipes stderr to [StandardError](/api/Vercel.AI.Mcp/StdioMcpTransport/#Vercel_AI_Mcp_StdioMcpTransport_StandardError). False lets the child inherit it.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L195)

<a id="Vercel_AI_Mcp_StdioMcpOptions_Shell_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_Shell"></a>

### Shell


```csharp
public bool Shell { get; set; }
```


True runs the command through a shell (`/bin/sh -c`, or `cmd.exe /d /s /c` on Windows), so command shims
such as the Windows `npx` work. Arguments are joined with spaces and are not quoted. Mirrors the upstream `shell` option.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L201)

<a id="Vercel_AI_Mcp_StdioMcpOptions_ShellPath_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_ShellPath"></a>

### ShellPath


```csharp
public string? ShellPath { get; set; }
```


The shell executable to use. Setting it implies [Shell](#Vercel_AI_Mcp_StdioMcpOptions_Shell). Mirrors the upstream `shell` string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L204)

<a id="Vercel_AI_Mcp_StdioMcpOptions_WorkingDirectory_"></a>
<a id="Vercel_AI_Mcp_StdioMcpOptions_WorkingDirectory"></a>

### WorkingDirectory


```csharp
public string? WorkingDirectory { get; set; }
```


Working directory. Null uses the current directory.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L192)
