---
title: "MediaTypeDetector"
description: "Detects media types from file signatures."
editUrl: false
prev: false
next: false
sidebar:
  label: "MediaTypeDetector"
  hidden: true
---

<a id="Vercel_AI_Operations_MediaTypeDetector"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class MediaTypeDetector
```


Detects media types from file signatures.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L711)

## Methods

<a id="Vercel_AI_Operations_MediaTypeDetector_Detect_"></a>
<a id="Vercel_AI_Operations_MediaTypeDetector_Detect_System_Byte___System_String_"></a>

### Detect(byte[]?, string?)


```csharp
public static string? Detect(byte[]? data, string? topLevelType)
```


Returns a media type for &lt;code class="paramref"&gt;data&lt;/code&gt;, or null when no signature matches.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `topLevelType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L714)

<a id="Vercel_AI_Operations_MediaTypeDetector_ToBytes_"></a>
<a id="Vercel_AI_Operations_MediaTypeDetector_ToBytes_System_Object_"></a>

### ToBytes(object?)


```csharp
public static byte[] ToBytes(object? data)
```


Decodes a base64 payload or returns the bytes unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L720)
