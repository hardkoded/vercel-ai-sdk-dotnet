---
title: "CanonicalHash"
description: "Canonical JSON and SHA-256 digests used by tool fingerprints and approval signatures."
editUrl: false
prev: false
next: false
sidebar:
  label: "CanonicalHash"
  hidden: true
---

<a id="Vercel_AI_GenerateText_CanonicalHash"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class CanonicalHash
```


Canonical JSON and SHA-256 digests used by tool fingerprints and approval signatures.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CanonicalHash.cs#L12)

## Methods

<a id="Vercel_AI_GenerateText_CanonicalHash_CanonicalJson_"></a>
<a id="Vercel_AI_GenerateText_CanonicalHash_CanonicalJson_System_Text_Json_JsonElement_"></a>

### CanonicalJson(JsonElement)


```csharp
public static string CanonicalJson(JsonElement value)
```


Serializes &lt;code class="paramref"&gt;value&lt;/code&gt; with sorted object keys.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CanonicalHash.cs#L15)

<a id="Vercel_AI_GenerateText_CanonicalHash_HashCanonical_"></a>
<a id="Vercel_AI_GenerateText_CanonicalHash_HashCanonical_System_Text_Json_JsonElement_"></a>

### HashCanonical(JsonElement)


```csharp
public static string HashCanonical(JsonElement value)
```


SHA-256 digest of [CanonicalJson(JsonElement)](#Vercel_AI_GenerateText_CanonicalHash_CanonicalJson_System_Text_Json_JsonElement_), encoded as unpadded base64url.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CanonicalHash.cs#L21)

<a id="Vercel_AI_GenerateText_CanonicalHash_ToBase64Url_"></a>
<a id="Vercel_AI_GenerateText_CanonicalHash_ToBase64Url_System_Byte___"></a>

### ToBase64Url(byte[])


```csharp
public static string ToBase64Url(byte[] bytes)
```


Encodes &lt;code class="paramref"&gt;bytes&lt;/code&gt; as unpadded base64url.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CanonicalHash.cs#L29)
