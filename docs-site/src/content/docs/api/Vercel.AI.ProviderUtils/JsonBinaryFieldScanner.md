---
title: "JsonBinaryFieldScanner"
description: "Streaming extraction of large base64 strings from a JSON response body. A JSON response that carries an image or a file is mostly one very long base64 string..."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonBinaryFieldScanner"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class JsonBinaryFieldScanner
```


Streaming extraction of large base64 strings from a JSON response body.
A JSON response that carries an image or a file is mostly one very long base64 string. The scanner reads the body
chunk by chunk: strings at the configured paths are decoded to bytes while they stream in, and the text that is
handed to the JSON parser (the "skeleton") holds a short placeholder in their place.
A string is extracted only if it is canonical standard-alphabet base64 (length divisible by 4, `=` only as the
final padding, zero trailing bits, no escape sequences or whitespace). Everything else stays a string.
Maps to `json-binary-field-scanner.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L53)

## Constructors

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner__ctor_Vercel_AI_ProviderUtils_BinaryFieldPaths_System_Int32_"></a>

### JsonBinaryFieldScanner(BinaryFieldPaths, int)


```csharp
public JsonBinaryFieldScanner(BinaryFieldPaths paths, int minBytes = 65536)
```


Creates a scanner for the compiled &lt;code class="paramref"&gt;paths&lt;/code&gt;. Maps to `createJsonBinaryFieldScanner`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `paths` | [BinaryFieldPaths](/api/Vercel.AI.ProviderUtils/BinaryFieldPaths/) |  |
| `minBytes` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L123)

## Fields

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_DefaultBinaryFieldMinBytes"></a>

### DefaultBinaryFieldMinBytes


```csharp
public const int DefaultBinaryFieldMinBytes = 65536
```


Strings that decode to fewer bytes stay strings. Maps to `DEFAULT_BINARY_FIELD_MIN_BYTES`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L56)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_CompileBinaryFieldPaths_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_CompileBinaryFieldPaths_System_Collections_Generic_IEnumerable_System_String__"></a>

### `CompileBinaryFieldPaths(IEnumerable<string>)`


```csharp
public static BinaryFieldPaths CompileBinaryFieldPaths(IEnumerable<string> paths)
```


Compiles paths such as `images[*]` or `content[*].data.data`: plain object keys (letters, digits, `_` and `-`)
joined by `.`, and `[*]` for every element of an array. Keys are compared with the raw bytes of the key in the body,
so a key written with an escape sequence never matches. Maps to `compileBinaryFieldPaths`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `paths` | [IEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |

**Returns:** [BinaryFieldPaths](/api/Vercel.AI.ProviderUtils/BinaryFieldPaths/)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | A path is not valid. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L138)

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_Finish_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_Finish"></a>

### Finish()


```csharp
public JsonBinaryFieldScanResult Finish()
```


Ends the body. A body that ends inside a candidate string is returned as it was received, so that the regular parser reports the truncation.
Maps to `finish`.

**Returns:** [JsonBinaryFieldScanResult](/api/Vercel.AI.ProviderUtils/JsonBinaryFieldScanResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L351)

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_Push_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_Push_System_Byte___System_Int32_"></a>

### Push(byte[], int)


```csharp
public void Push(byte[] chunk, int count)
```


Reads the next chunk of the body. Maps to `push`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `count` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L185)

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_ReplaceBinaryFieldPlaceholders_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanner_ReplaceBinaryFieldPlaceholders_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Byte____"></a>

### `ReplaceBinaryFieldPlaceholders(JsonElement, IReadOnlyDictionary<string, byte[]>)`


```csharp
public static object? ReplaceBinaryFieldPlaceholders(JsonElement value, IReadOnlyDictionary<string, byte[]> binaries)
```


Replaces the placeholders in a parsed value with the bytes they stand for. Objects and arrays are copied, the parsed value is not modified.
The result holds `null`, [bool](https://learn.microsoft.com/dotnet/api/system.boolean), [double](https://learn.microsoft.com/dotnet/api/system.double), [string](https://learn.microsoft.com/dotnet/api/system.string), [byte](https://learn.microsoft.com/dotnet/api/system.byte) arrays,
`List%601` of objects, and `Dictionary%602` of objects. A placeholder holds a random number,
so a string that equals one is ours. Maps to `replaceBinaryFieldPlaceholders`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `binaries` | [IReadOnlyDictionary&lt;string, byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L368)
