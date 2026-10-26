---
title: "MediaTypes"
description: "Detects IANA media types from file signatures. Maps to detectMediaType."
editUrl: false
prev: false
next: false
sidebar:
  label: "MediaTypes"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_MediaTypes"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class MediaTypes
```


Detects IANA media types from file signatures. Maps to `detectMediaType`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L11)

## Fields

<a id="Vercel_AI_ProviderUtils_MediaTypes_MaxId3TagBytes"></a>

### MaxId3TagBytes


```csharp
public const int MaxId3TagBytes = 131072
```


Largest ID3v2 tag skipped before the audio frame.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L14)

## Methods

<a id="Vercel_AI_ProviderUtils_MediaTypes_DetectMediaType_"></a>
<a id="Vercel_AI_ProviderUtils_MediaTypes_DetectMediaType_System_Byte___System_String_"></a>

### DetectMediaType(byte[], string?)


```csharp
public static string? DetectMediaType(byte[] data, string? topLevelType = null)
```


Detects a media type from raw bytes or a base64 string.
When &lt;code class="paramref"&gt;topLevelType&lt;/code&gt; is omitted, image, document, audio, and video signatures are considered.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `topLevelType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L74)

<a id="Vercel_AI_ProviderUtils_MediaTypes_DetectMediaType_System_String_System_String_"></a>

### DetectMediaType(string, string?)


```csharp
public static string? DetectMediaType(string base64, string? topLevelType = null)
```


Detects a media type from a base64 string. See `String)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `base64` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `topLevelType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L80)

<a id="Vercel_AI_ProviderUtils_MediaTypes_GetTopLevelMediaType_"></a>
<a id="Vercel_AI_ProviderUtils_MediaTypes_GetTopLevelMediaType_System_String_"></a>

### GetTopLevelMediaType(string)


```csharp
public static string GetTopLevelMediaType(string mediaType)
```


Returns the segment before the first slash. Maps to `getTopLevelMediaType`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L119)

<a id="Vercel_AI_ProviderUtils_MediaTypes_IsFullMediaType_"></a>
<a id="Vercel_AI_ProviderUtils_MediaTypes_IsFullMediaType_System_String_"></a>

### IsFullMediaType(string)


```csharp
public static bool IsFullMediaType(string mediaType)
```


True for `type/subtype` when the subtype is non-empty and not `*`. Maps to `isFullMediaType`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L131)

<a id="Vercel_AI_ProviderUtils_MediaTypes_ResolveFullMediaType_"></a>
<a id="Vercel_AI_ProviderUtils_MediaTypes_ResolveFullMediaType_System_String_System_Byte___"></a>

### ResolveFullMediaType(string, byte[]?)


```csharp
public static string ResolveFullMediaType(string mediaType, byte[]? inlineData)
```


Resolves a media type for inline bytes, or null when the file is not inline. See [ResolveFullMediaType(LanguageModelFilePart)](#Vercel_AI_ProviderUtils_MediaTypes_ResolveFullMediaType_Vercel_AI_Prompt_LanguageModelFilePart_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `inlineData` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L165)

<a id="Vercel_AI_ProviderUtils_MediaTypes_ResolveFullMediaType_Vercel_AI_Prompt_LanguageModelFilePart_"></a>

### ResolveFullMediaType(LanguageModelFilePart)


```csharp
public static string ResolveFullMediaType(LanguageModelFilePart file)
```


Returns the file part media type when it has a subtype. Otherwise detects the subtype from the file part data,
which is the file part's bytes or base64 text, or null when the file is not inline. Maps to `resolveFullMediaType`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [LanguageModelFilePart](/api/Vercel.AI.Prompt/LanguageModelFilePart/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

**Exceptions**

| Type | Condition |
| --- | --- |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | The subtype cannot be resolved. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DetectMediaType.cs#L153)
