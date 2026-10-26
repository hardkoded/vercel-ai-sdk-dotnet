---
title: "Collections"
description: "Array and property helpers from the AI SDK util package."
editUrl: false
prev: false
next: false
sidebar:
  label: "Collections"
  hidden: true
---

<a id="Vercel_AI_Util_Collections"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class Collections
```


Array and property helpers from the AI SDK util package.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L8)

## Methods

<a id="Vercel_AI_Util_Collections_GetOwn_"></a>
<a id="Vercel_AI_Util_Collections_GetOwn_System_Collections_Generic_IDictionary_System_String_System_Object__System_String_"></a>

### `GetOwn(IDictionary<string, object>?, string?)`


```csharp
public static object? GetOwn(IDictionary<string, object>? obj, string? key)
```


Returns an own dictionary entry, or `null` when the key is absent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `obj` | [IDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |
| `key` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L55)

<a id="Vercel_AI_Util_Collections_GetOwn_Vercel_AI_Util_DataObject_System_String_"></a>

### GetOwn(DataObject?, string?)


```csharp
public static object? GetOwn(DataObject? obj, string? key)
```


Returns an own property value. Inherited names such as `constructor` are not returned.
Null and undefined objects return `null`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `obj` | [DataObject](/api/Vercel.AI.Util/DataObject/) |  |
| `key` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L43)

<a id="Vercel_AI_Util_Collections_GetPotentialStartIndex_"></a>
<a id="Vercel_AI_Util_Collections_GetPotentialStartIndex_System_String_System_String_"></a>

### GetPotentialStartIndex(string?, string?)


```csharp
public static int? GetPotentialStartIndex(string? text, string? searchedText)
```


Largest index where &lt;code class="paramref"&gt;searchedText&lt;/code&gt; occurs, or where a suffix of
&lt;code class="paramref"&gt;text&lt;/code&gt; is a prefix of &lt;code class="paramref"&gt;searchedText&lt;/code&gt;. Empty search text returns `null`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `searchedText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L70)

<a id="Vercel_AI_Util_Collections_SplitArray_"></a>
<a id="Vercel_AI_Util_Collections_SplitArray__1_System_Collections_Generic_IReadOnlyList___0__System_Int32_"></a>

### `SplitArray<T>(IReadOnlyList<T>?, int)`


```csharp
public static List<List<T>> SplitArray<T>(IReadOnlyList<T>? array, int chunkSize)
```


Splits &lt;code class="paramref"&gt;array&lt;/code&gt; into chunks of &lt;code class="paramref"&gt;chunkSize&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `array` | [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `chunkSize` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [List&lt;List&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L11)
