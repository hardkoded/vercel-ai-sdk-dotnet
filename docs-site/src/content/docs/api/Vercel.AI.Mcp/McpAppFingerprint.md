---
title: "McpAppFingerprint"
description: "SHA-256 fingerprint of an MCP app resource."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpAppFingerprint"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpAppFingerprint"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class McpAppFingerprint
```


SHA-256 fingerprint of an MCP app resource.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L646)

## Methods

<a id="Vercel_AI_Mcp_McpAppFingerprint_DetectDrift_"></a>
<a id="Vercel_AI_Mcp_McpAppFingerprint_DetectDrift_System_String_System_String_"></a>

### DetectDrift(string, string)


```csharp
public static bool DetectDrift(string current, string baseline)
```


True when the current fingerprint differs from the baseline.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `current` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseline` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L660)

<a id="Vercel_AI_Mcp_McpAppFingerprint_Fingerprint_"></a>
<a id="Vercel_AI_Mcp_McpAppFingerprint_Fingerprint_System_String_System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Text_Json_JsonElement__"></a>

### Fingerprint(string, JsonElement?, JsonElement?)


```csharp
public static string Fingerprint(string html, JsonElement? csp, JsonElement? permissions)
```


Stable base64url SHA-256 of html, csp, and permissions. Object keys are sorted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `html` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `csp` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `permissions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L649)
