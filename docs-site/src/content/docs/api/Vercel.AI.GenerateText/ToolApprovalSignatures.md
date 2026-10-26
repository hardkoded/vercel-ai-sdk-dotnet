---
title: "ToolApprovalSignatures"
description: "HMAC approval signatures. Maps to signToolApproval and verifyToolApprovalSignature."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalSignatures"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalSignatures"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolApprovalSignatures
```


HMAC approval signatures. Maps to `signToolApproval` and `verifyToolApprovalSignature`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalSignature.cs#L12)

## Methods

<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_MaybeSignApproval_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_MaybeSignApproval_System_String_System_String_System_String_System_String_System_Text_Json_JsonElement_"></a>

### MaybeSignApproval(string?, string, string, string, JsonElement)


```csharp
public static string? MaybeSignApproval(string? secret, string approvalId, string toolCallId, string toolName, JsonElement input)
```


Returns a signature when &lt;code class="paramref"&gt;secret&lt;/code&gt; is set, and null when it is not.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secret` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalSignature.cs#L39)

<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_SignToolApproval_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_SignToolApproval_System_Byte___System_String_System_String_System_String_System_Text_Json_JsonElement_"></a>

### SignToolApproval(byte[], string, string, string, JsonElement)


```csharp
public static string SignToolApproval(byte[] secret, string approvalId, string toolCallId, string toolName, JsonElement input)
```


Signs a tool approval with a raw secret.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secret` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalSignature.cs#L28)

<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_SignToolApproval_System_String_System_String_System_String_System_String_System_Text_Json_JsonElement_"></a>

### SignToolApproval(string, string, string, string, JsonElement)


```csharp
public static string SignToolApproval(string secret, string approvalId, string toolCallId, string toolName, JsonElement input)
```


Signs a tool approval. Equivalent inputs with different key order produce the same signature.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secret` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalSignature.cs#L17)

<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_VerifyToolApprovalSignature_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_VerifyToolApprovalSignature_System_Byte___System_String_System_String_System_String_System_String_System_Text_Json_JsonElement_"></a>

### VerifyToolApprovalSignature(byte[], string, string, string, string, JsonElement)


```csharp
public static bool VerifyToolApprovalSignature(byte[] secret, string signature, string approvalId, string toolCallId, string toolName, JsonElement input)
```


Verifies a signature with a raw secret.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secret` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `signature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalSignature.cs#L64)

<a id="Vercel_AI_GenerateText_ToolApprovalSignatures_VerifyToolApprovalSignature_System_String_System_String_System_String_System_String_System_String_System_Text_Json_JsonElement_"></a>

### VerifyToolApprovalSignature(string, string, string, string, string, JsonElement)


```csharp
public static bool VerifyToolApprovalSignature(string secret, string signature, string approvalId, string toolCallId, string toolName, JsonElement input)
```


Verifies a signature. Accepts the current JSON payload and, when no field contains a newline,
a legacy newline-joined payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secret` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `signature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalSignature.cs#L53)
