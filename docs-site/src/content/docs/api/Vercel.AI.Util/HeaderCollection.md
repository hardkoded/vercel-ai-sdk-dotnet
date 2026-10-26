---
title: "HeaderCollection"
description: "Case-insensitive header list that preserves repeated values such as Set-Cookie."
editUrl: false
prev: false
next: false
sidebar:
  label: "HeaderCollection"
  hidden: true
---

<a id="Vercel_AI_Util_HeaderCollection"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class HeaderCollection
```


Case-insensitive header list that preserves repeated values such as `Set-Cookie`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L8)

## Constructors

<a id="Vercel_AI_Util_HeaderCollection__ctor_"></a>
<a id="Vercel_AI_Util_HeaderCollection__ctor"></a>

### HeaderCollection()


```csharp
public HeaderCollection()
```


Creates an empty collection.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L13)

<a id="Vercel_AI_Util_HeaderCollection__ctor_System_Collections_Generic_IEnumerable_System_Collections_Generic_KeyValuePair_System_String_System_String___"></a>

### `HeaderCollection(IEnumerable<KeyValuePair<string, string>>?)`


```csharp
public HeaderCollection(IEnumerable<KeyValuePair<string, string>>? headers)
```


Copies &lt;code class="paramref"&gt;headers&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IEnumerable&lt;KeyValuePair&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L18)

## Methods

<a id="Vercel_AI_Util_HeaderCollection_Add_"></a>
<a id="Vercel_AI_Util_HeaderCollection_Add_System_String_System_String_"></a>

### Add(string, string)


```csharp
public void Add(string name, string value)
```


Appends one header value.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L32)

<a id="Vercel_AI_Util_HeaderCollection_Contains_"></a>
<a id="Vercel_AI_Util_HeaderCollection_Contains_System_String_"></a>

### Contains(string)


```csharp
public bool Contains(string name)
```


Returns whether a header with &lt;code class="paramref"&gt;name&lt;/code&gt; is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L38)

<a id="Vercel_AI_Util_HeaderCollection_Entries_"></a>
<a id="Vercel_AI_Util_HeaderCollection_Entries"></a>

### Entries()


```csharp
public IReadOnlyList<KeyValuePair<string, string>> Entries()
```


Header entries in insertion order.

**Returns:** [IReadOnlyList&lt;KeyValuePair&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L81)

<a id="Vercel_AI_Util_HeaderCollection_Get_"></a>
<a id="Vercel_AI_Util_HeaderCollection_Get_System_String_"></a>

### Get(string)


```csharp
public string? Get(string name)
```


First value for &lt;code class="paramref"&gt;name&lt;/code&gt;, or `null`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L52)

<a id="Vercel_AI_Util_HeaderCollection_GetAll_"></a>
<a id="Vercel_AI_Util_HeaderCollection_GetAll_System_String_"></a>

### GetAll(string)


```csharp
public IReadOnlyList<string> GetAll(string name)
```


Every value for &lt;code class="paramref"&gt;name&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L66)
