---
title: "ByteEncoding"
description: "Base64 conversions for inline file bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "ByteEncoding"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ByteEncoding"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ByteEncoding
```


Base64 conversions for inline file bytes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L483)

## Methods

<a id="Vercel_AI_ProviderUtils_ByteEncoding_ConvertToBase64_"></a>
<a id="Vercel_AI_ProviderUtils_ByteEncoding_ConvertToBase64_System_Byte___"></a>

### ConvertToBase64(byte[])


```csharp
public static string ConvertToBase64(byte[] value)
```


Encodes bytes as base64.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L526)

<a id="Vercel_AI_ProviderUtils_ByteEncoding_ConvertToBase64_System_String_"></a>

### ConvertToBase64(string)


```csharp
public static string ConvertToBase64(string value)
```


Returns base64 text unchanged, or encodes bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L520)

<a id="Vercel_AI_ProviderUtils_ByteEncoding_FromBase64_"></a>
<a id="Vercel_AI_ProviderUtils_ByteEncoding_FromBase64_System_String_"></a>

### FromBase64(string)


```csharp
public static byte[] FromBase64(string text)
```


Decodes standard or base64url text. An empty string decodes to an empty array.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L497)

<a id="Vercel_AI_ProviderUtils_ByteEncoding_ToBase64_"></a>
<a id="Vercel_AI_ProviderUtils_ByteEncoding_ToBase64_System_Byte___"></a>

### ToBase64(byte[])


```csharp
public static string ToBase64(byte[] bytes)
```


Encodes bytes as standard base64.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L486)
