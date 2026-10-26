---
title: "StdioEnvironment"
description: "Environment passed to an MCP stdio child. The caller's dictionary is not modified."
editUrl: false
prev: false
next: false
sidebar:
  label: "StdioEnvironment"
  hidden: true
---

<a id="Vercel_AI_Mcp_StdioEnvironment"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class StdioEnvironment
```


Environment passed to an MCP stdio child. The caller's dictionary is not modified.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L767)

## Methods

<a id="Vercel_AI_Mcp_StdioEnvironment_GetEnvironment_"></a>
<a id="Vercel_AI_Mcp_StdioEnvironment_GetEnvironment_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Boolean__"></a>

### `GetEnvironment(IReadOnlyDictionary<string, string>?, bool?)`


```csharp
public static Dictionary<string, string> GetEnvironment(IReadOnlyDictionary<string, string>? custom, bool? windows = null)
```


Copies &lt;code class="paramref"&gt;custom&lt;/code&gt; and fills inherited process variables that do not start with `()`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `custom` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `windows` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L776)

<a id="Vercel_AI_Mcp_StdioEnvironment_ValidateCommand_"></a>
<a id="Vercel_AI_Mcp_StdioEnvironment_ValidateCommand_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Boolean_"></a>

### `ValidateCommand(string, IReadOnlyList<string>?, bool)`


```csharp
public static void ValidateCommand(string command, IReadOnlyList<string>? arguments, bool windows)
```


Rejects command and argument line breaks on Windows.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `command` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `arguments` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `windows` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L803)
