---
title: "FixJson"
description: "Repairs truncated JSON by scanning once and closing open strings, literals, arrays, and objects."
editUrl: false
prev: false
next: false
sidebar:
  label: "FixJson"
  hidden: true
---

<a id="Vercel_AI_Util_FixJson"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class FixJson
```


Repairs truncated JSON by scanning once and closing open strings, literals, arrays, and objects.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/FixJson.cs#L10)

## Methods

<a id="Vercel_AI_Util_FixJson_Repair_"></a>
<a id="Vercel_AI_Util_FixJson_Repair_System_String_"></a>

### Repair(string?)


```csharp
public static string Repair(string? input)
```


Returns &lt;code class="paramref"&gt;input&lt;/code&gt; with truncated JSON closed enough for a standard parser.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `input` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/FixJson.cs#L30)
