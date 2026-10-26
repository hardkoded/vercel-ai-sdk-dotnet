---
title: "CanonicalHash"
description: "Canonical JSON and its SHA-256 base64url digest."
editUrl: false
prev: false
next: false
sidebar:
  label: "CanonicalHash"
  hidden: true
---

<a id="Vercel_AI_Util_CanonicalHash"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class CanonicalHash
```


Canonical JSON and its SHA-256 base64url digest.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/CanonicalHash.cs#L13)

## Methods

<a id="Vercel_AI_Util_CanonicalHash_CanonicalJson_"></a>
<a id="Vercel_AI_Util_CanonicalHash_CanonicalJson_System_Object_"></a>

### CanonicalJson(object?)


```csharp
public static string? CanonicalJson(object? value)
```


Serializes &lt;code class="paramref"&gt;value&lt;/code&gt; with object keys sorted.
[Value](/api/Vercel.AI.Util/JsUndefined/#Vercel_AI_Util_JsUndefined_Value) returns `null`, matching `JSON.stringify(undefined)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/CanonicalHash.cs#L19)

<a id="Vercel_AI_Util_CanonicalHash_HashCanonical_"></a>
<a id="Vercel_AI_Util_CanonicalHash_HashCanonical_System_Object_"></a>

### HashCanonical(object?)


```csharp
public static string HashCanonical(object? value)
```


SHA-256 digest of [CanonicalJson(object)](#Vercel_AI_Util_CanonicalHash_CanonicalJson_System_Object_), encoded as base64url without padding.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/CanonicalHash.cs#L75)

<a id="Vercel_AI_Util_CanonicalHash_ToBase64Url_"></a>
<a id="Vercel_AI_Util_CanonicalHash_ToBase64Url_System_Byte___"></a>

### ToBase64Url(byte[])


```csharp
public static string ToBase64Url(byte[] bytes)
```


Encodes &lt;code class="paramref"&gt;bytes&lt;/code&gt; as base64url without padding.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/CanonicalHash.cs#L86)
