---
title: "PrepareHeaders"
description: "Fills default headers without replacing headers the caller already set."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareHeaders"
  hidden: true
---

<a id="Vercel_AI_Util_PrepareHeaders"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class PrepareHeaders
```


Fills default headers without replacing headers the caller already set.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L88)

## Methods

<a id="Vercel_AI_Util_PrepareHeaders_Apply_"></a>
<a id="Vercel_AI_Util_PrepareHeaders_Apply_Vercel_AI_Util_HeaderCollection_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Apply(HeaderCollection?, IReadOnlyDictionary<string, string>?)`


```csharp
public static HeaderCollection Apply(HeaderCollection? headers, IReadOnlyDictionary<string, string>? defaultHeaders)
```


Copies &lt;code class="paramref"&gt;headers&lt;/code&gt; and sets each default that is not already present.
Header names are compared case-insensitively.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `defaultHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/HeaderCollection.cs#L94)
