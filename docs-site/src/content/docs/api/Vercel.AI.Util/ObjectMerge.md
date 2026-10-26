---
title: "ObjectMerge"
description: "Deep-merges JSON objects. Arrays, dates, and regular expressions are replaced."
editUrl: false
prev: false
next: false
sidebar:
  label: "ObjectMerge"
  hidden: true
---

<a id="Vercel_AI_Util_ObjectMerge"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class ObjectMerge
```


Deep-merges JSON objects. Arrays, dates, and regular expressions are replaced.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ObjectMerge.cs#L11)

## Methods

<a id="Vercel_AI_Util_ObjectMerge_MergeObjects_"></a>
<a id="Vercel_AI_Util_ObjectMerge_MergeObjects_System_Collections_Generic_IDictionary_System_String_System_Object__System_Collections_Generic_IDictionary_System_String_System_Object__"></a>

### `MergeObjects(IDictionary<string, object>?, IDictionary<string, object>?)`


```csharp
public static IDictionary<string, object>? MergeObjects(IDictionary<string, object>? baseObject, IDictionary<string, object>? overrides)
```


Merges &lt;code class="paramref"&gt;overrides&lt;/code&gt; onto a copy of &lt;code class="paramref"&gt;baseObject&lt;/code&gt;.
Undefined override values are skipped. `__proto__`, `constructor`, and `prototype` are ignored.
Returns `null` when both inputs are null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseObject` | [IDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |
| `overrides` | [IDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |

**Returns:** [IDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ObjectMerge.cs#L18)
