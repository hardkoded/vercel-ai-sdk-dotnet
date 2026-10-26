---
title: "ProviderValues"
description: "Small collection, header, text, and media helpers shared by providers."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderValues"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderValues"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ProviderValues
```


Small collection, header, text, and media helpers shared by providers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L12)

## Methods

<a id="Vercel_AI_ProviderUtils_ProviderValues_AsArray_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_AsArray__1_System_Collections_Generic_IReadOnlyList___0__"></a>

### `AsArray<T>(IReadOnlyList<T>?)`


```csharp
public static IReadOnlyList<T> AsArray<T>(IReadOnlyList<T>? value)
```


Returns an empty list for null, or the same list.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L22)

<a id="Vercel_AI_ProviderUtils_ProviderValues_AsArray__1___0_"></a>

### `AsArray<T>(T?)`


```csharp
public static IReadOnlyList<T> AsArray<T>(T? value) where T : class
```


Returns an empty list for null, or a one-item list.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

**Returns:** [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L15)

<a id="Vercel_AI_ProviderUtils_ProviderValues_ExtractLines_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_ExtractLines_System_String_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### ExtractLines(string, int?, int?)


```csharp
public static string ExtractLines(string text, int? startLine = null, int? endLine = null)
```


Inclusive 1-based line slice. Line endings are detected as `\r\n`, then `\n`, then `\r`.
When both bounds are null the text is returned unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `startLine` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `endLine` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L403)

<a id="Vercel_AI_ProviderUtils_ProviderValues_FilterNullable_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_FilterNullable__1___0___"></a>

### `FilterNullable<T>(params T?[])`


```csharp
public static IReadOnlyList<T> FilterNullable<T>(params T?[] values)
```


Drops null entries and keeps other values, including false, zero, and empty strings.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | `T\[\]` |  |

**Returns:** [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L28)

<a id="Vercel_AI_ProviderUtils_ProviderValues_GetRuntimeEnvironmentUserAgent_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_GetRuntimeEnvironmentUserAgent_Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_"></a>

### GetRuntimeEnvironmentUserAgent(RuntimeProbe?)


```csharp
public static string GetRuntimeEnvironmentUserAgent(ProviderValues.RuntimeProbe? probe = null)
```


Describes the runtime the way provider user-agent strings do.
Window is checked first, then navigator, then Node, then the Edge runtime.
Returns an empty string when the runtime is unknown, so callers can omit it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `probe` | [ProviderValues.RuntimeProbe](/api/Vercel.AI.ProviderUtils/ProviderValues.RuntimeProbe/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L294)

<a id="Vercel_AI_ProviderUtils_ProviderValues_IsBrowserRuntime_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_IsBrowserRuntime_Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_"></a>

### IsBrowserRuntime(RuntimeProbe?)


```csharp
public static bool IsBrowserRuntime(ProviderValues.RuntimeProbe? probe = null)
```


True when the probe reports a browser `window`. Maps to `isBrowserRuntime`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `probe` | [ProviderValues.RuntimeProbe](/api/Vercel.AI.ProviderUtils/ProviderValues.RuntimeProbe/) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L325)

<a id="Vercel_AI_ProviderUtils_ProviderValues_IsJsonSerializable_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_IsJsonSerializable_System_Object_"></a>

### IsJsonSerializable(object?)


```csharp
public static bool IsJsonSerializable(object? value)
```


True when a value can be written as JSON: null, primitives, arrays, and string-keyed dictionaries.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L79)

<a id="Vercel_AI_ProviderUtils_ProviderValues_IsRecord_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_IsRecord_System_Object_"></a>

### IsRecord(object?)


```csharp
public static bool IsRecord(object? value)
```


True for a non-null object that is not a string, primitive, or list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L68)

<a id="Vercel_AI_ProviderUtils_ProviderValues_IsSameOrigin_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_IsSameOrigin_System_String_System_String_"></a>

### IsSameOrigin(string, string)


```csharp
public static bool IsSameOrigin(string url, string baseUrl)
```


True when both absolute URLs have the same scheme, host, and port.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L239)

<a id="Vercel_AI_ProviderUtils_ProviderValues_MediaTypeToExtension_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_MediaTypeToExtension_System_String_"></a>

### MediaTypeToExtension(string)


```csharp
public static string MediaTypeToExtension(string mediaType)
```


Maps an audio media type to a file extension. Unknown types use the subtype, or empty text when there is no slash.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L264)

<a id="Vercel_AI_ProviderUtils_ProviderValues_NormalizeBatchRequestCounts_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_NormalizeBatchRequestCounts_System_Nullable_System_Double__System_Nullable_System_Double__System_Nullable_System_Double__System_Nullable_System_Double__"></a>

### NormalizeBatchRequestCounts(double?, double?, double?, double?)


```csharp
public static ProviderValues.BatchRequestCounts? NormalizeBatchRequestCounts(double? total, double? pending, double? completed, double? failed)
```


Returns counts when every value is a non-negative safe integer and the parts add up to the total.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `total` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `pending` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `completed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `failed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

**Returns:** [ProviderValues.BatchRequestCounts](/api/Vercel.AI.ProviderUtils/ProviderValues.BatchRequestCounts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L377)

<a id="Vercel_AI_ProviderUtils_ProviderValues_NormalizeHeaders_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_NormalizeHeaders_System_Collections_Generic_IEnumerable_System_Collections_Generic_KeyValuePair_System_String_System_String___"></a>

### `NormalizeHeaders(IEnumerable<KeyValuePair<string, string?>>?)`


```csharp
public static Dictionary<string, string> NormalizeHeaders(IEnumerable<KeyValuePair<string, string?>>? headers)
```


Lower-cases header names and drops null values.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IEnumerable&lt;KeyValuePair&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L165)

<a id="Vercel_AI_ProviderUtils_ProviderValues_RemoveUndefinedEntries_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_RemoveUndefinedEntries__1_System_Collections_Generic_IReadOnlyDictionary_System_String___0__"></a>

### `RemoveUndefinedEntries<T>(IReadOnlyDictionary<string, T?>)`


```csharp
public static Dictionary<string, T> RemoveUndefinedEntries<T>(IReadOnlyDictionary<string, T?> record)
```


Copies entries whose values are not null.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `record` | [IReadOnlyDictionary&lt;string, T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Dictionary&lt;string, T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L48)

<a id="Vercel_AI_ProviderUtils_ProviderValues_StripFileExtension_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_StripFileExtension_System_String_"></a>

### StripFileExtension(string)


```csharp
public static string StripFileExtension(string filename)
```


Returns the substring before the first dot. A trailing dot is removed with the empty extension.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `filename` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L252)

<a id="Vercel_AI_ProviderUtils_ProviderValues_ValidateBaseUrl_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_ValidateBaseUrl_System_String_"></a>

### ValidateBaseUrl(string?)


```csharp
public static string? ValidateBaseUrl(string? baseUrl)
```


Returns &lt;code class="paramref"&gt;baseUrl&lt;/code&gt;, or throws when it is empty or whitespace.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L228)

<a id="Vercel_AI_ProviderUtils_ProviderValues_WithUserAgentSuffix_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_WithUserAgentSuffix_System_Collections_Generic_IEnumerable_System_Collections_Generic_KeyValuePair_System_String_System_String___System_String___"></a>

### `WithUserAgentSuffix(IEnumerable<KeyValuePair<string, string?>>?, params string[])`


```csharp
public static Dictionary<string, string> WithUserAgentSuffix(IEnumerable<KeyValuePair<string, string?>>? headers, params string[] suffixParts)
```


Appends &lt;code class="paramref"&gt;suffixParts&lt;/code&gt; to the user-agent header, creating it when needed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IEnumerable&lt;KeyValuePair&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |
| `suffixParts` | [string\[\]](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L185)

<a id="Vercel_AI_ProviderUtils_ProviderValues_WithoutTrailingSlash_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_WithoutTrailingSlash_System_String_"></a>

### WithoutTrailingSlash(string?)


```csharp
public static string? WithoutTrailingSlash(string? url)
```


Removes one trailing slash.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L212)
