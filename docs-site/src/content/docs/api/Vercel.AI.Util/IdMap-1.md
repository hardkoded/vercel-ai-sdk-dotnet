---
title: "IdMap<T>"
description: "String-keyed map with no inherited members. Missing keys, including proto, return the default value."
editUrl: false
prev: false
next: false
sidebar:
  label: "IdMap<T>"
  hidden: true
---

<a id="Vercel_AI_Util_IdMap_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class IdMap<T>
```


String-keyed map with no inherited members. Missing keys, including `__proto__`, return the default value.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L104)

## Properties

<a id="Vercel_AI_Util_IdMap_1_Keys_"></a>
<a id="Vercel_AI_Util_IdMap_1_Keys"></a>

### Keys


```csharp
public IReadOnlyList<string> Keys { get; }
```


Own keys in insertion order.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L116)

<a id="Vercel_AI_Util_IdMap_1_Prototype_"></a>
<a id="Vercel_AI_Util_IdMap_1_Prototype"></a>

### Prototype


```csharp
public object? Prototype { get; }
```


No prototype object is consulted. Always `null`.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L110)

## Methods

<a id="Vercel_AI_Util_IdMap_1_Get_"></a>
<a id="Vercel_AI_Util_IdMap_1_Get_System_String_"></a>

### Get(string?)


```csharp
public T? Get(string? key)
```


Returns the own value, or `default` when &lt;code class="paramref"&gt;key&lt;/code&gt; is absent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `key` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L133)

<a id="Vercel_AI_Util_IdMap_1_Has_"></a>
<a id="Vercel_AI_Util_IdMap_1_Has_System_String_"></a>

### Has(string?)


```csharp
public bool Has(string? key)
```


Returns whether &lt;code class="paramref"&gt;key&lt;/code&gt; is an own key.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `key` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L144)

<a id="Vercel_AI_Util_IdMap_1_Set_"></a>
<a id="Vercel_AI_Util_IdMap_1_Set_System_String__0_"></a>

### Set(string, T)


```csharp
public void Set(string key, T value)
```


Stores &lt;code class="paramref"&gt;value&lt;/code&gt; under &lt;code class="paramref"&gt;key&lt;/code&gt; as an own entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `key` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Collections.cs#L122)
