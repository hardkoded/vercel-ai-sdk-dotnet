---
title: "FilePartInput"
description: "A file-part value before it is converted to the v4 provider shape."
editUrl: false
prev: false
next: false
sidebar:
  label: "FilePartInput"
  hidden: true
---

<a id="Vercel_AI_Prompt_FilePartInput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class FilePartInput
```


A file-part value before it is converted to the v4 provider shape.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L10)

## Properties

<a id="Vercel_AI_Prompt_FilePartInput_Bytes_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_Bytes"></a>

### Bytes


```csharp
public byte[]? Bytes { get; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L20)

<a id="Vercel_AI_Prompt_FilePartInput_IsArrayBuffer_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_IsArrayBuffer"></a>

### IsArrayBuffer


```csharp
public bool IsArrayBuffer { get; }
```


True when [Bytes](#Vercel_AI_Prompt_FilePartInput_Bytes) came from an ArrayBuffer and should be copied.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L23)

<a id="Vercel_AI_Prompt_FilePartInput_Reference_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_Reference"></a>

### Reference


```csharp
public IReadOnlyDictionary<string, string>? Reference { get; }
```


Provider reference.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L35)

<a id="Vercel_AI_Prompt_FilePartInput_Text_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Text payload.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L38)

<a id="Vercel_AI_Prompt_FilePartInput_TextData_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_TextData"></a>

### TextData


```csharp
public string? TextData { get; }
```


Inline base64 or any non-URL string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L26)

<a id="Vercel_AI_Prompt_FilePartInput_Type_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Tagged type, when this value is already tagged.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L17)

<a id="Vercel_AI_Prompt_FilePartInput_Url_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_Url"></a>

### Url


```csharp
public Uri? Url { get; }
```


URL instance.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L29)

<a id="Vercel_AI_Prompt_FilePartInput_UrlText_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_UrlText"></a>

### UrlText


```csharp
public string? UrlText { get; }
```


URL string that has not been parsed yet.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L32)

## Methods

<a id="Vercel_AI_Prompt_FilePartInput_FromArrayBuffer_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_FromArrayBuffer_System_Byte___"></a>

### FromArrayBuffer(byte[])


```csharp
public static FilePartInput FromArrayBuffer(byte[] bytes)
```


Creates an ArrayBuffer payload. The bytes are copied during conversion.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L47)

<a id="Vercel_AI_Prompt_FilePartInput_FromBytes_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_FromBytes_System_Byte___"></a>

### FromBytes(byte[])


```csharp
public static FilePartInput FromBytes(byte[] bytes)
```


Creates inline bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L41)

<a id="Vercel_AI_Prompt_FilePartInput_FromReference_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_FromReference_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `FromReference(IReadOnlyDictionary<string, string>)`


```csharp
public static FilePartInput FromReference(IReadOnlyDictionary<string, string> reference)
```


Creates a provider reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L65)

<a id="Vercel_AI_Prompt_FilePartInput_FromString_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_FromString_System_String_"></a>

### FromString(string)


```csharp
public static FilePartInput FromString(string value)
```


Creates a string payload. URLs and data URLs are detected during conversion.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L53)

<a id="Vercel_AI_Prompt_FilePartInput_FromUrl_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_FromUrl_System_Uri_"></a>

### FromUrl(Uri)


```csharp
public static FilePartInput FromUrl(Uri url)
```


Creates a URL payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L59)

<a id="Vercel_AI_Prompt_FilePartInput_TaggedData_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_TaggedData_System_Byte___"></a>

### TaggedData(byte[])


```csharp
public static FilePartInput TaggedData(byte[] bytes)
```


Creates tagged inline bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L71)

<a id="Vercel_AI_Prompt_FilePartInput_TaggedData_System_String_"></a>

### TaggedData(string)


```csharp
public static FilePartInput TaggedData(string data)
```


Creates tagged inline text. A data URL is rejected.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L83)

<a id="Vercel_AI_Prompt_FilePartInput_TaggedDataBuffer_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_TaggedDataBuffer_System_Byte___"></a>

### TaggedDataBuffer(byte[])


```csharp
public static FilePartInput TaggedDataBuffer(byte[] bytes)
```


Creates tagged inline bytes copied from an ArrayBuffer.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bytes` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L77)

<a id="Vercel_AI_Prompt_FilePartInput_TaggedReference_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_TaggedReference_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `TaggedReference(IReadOnlyDictionary<string, string>)`


```csharp
public static FilePartInput TaggedReference(IReadOnlyDictionary<string, string> reference)
```


Creates a tagged provider reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L95)

<a id="Vercel_AI_Prompt_FilePartInput_TaggedText_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_TaggedText_System_String_"></a>

### TaggedText(string)


```csharp
public static FilePartInput TaggedText(string text)
```


Creates a tagged text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L101)

<a id="Vercel_AI_Prompt_FilePartInput_TaggedUrl_"></a>
<a id="Vercel_AI_Prompt_FilePartInput_TaggedUrl_System_Uri_"></a>

### TaggedUrl(Uri)


```csharp
public static FilePartInput TaggedUrl(Uri url)
```


Creates a tagged URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |

**Returns:** [FilePartInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L89)
