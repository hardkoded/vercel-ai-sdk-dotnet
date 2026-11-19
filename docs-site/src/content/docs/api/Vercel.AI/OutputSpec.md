---
title: "OutputSpec"
description: "Structured output selector. Maps to Output."
editUrl: false
prev: false
next: false
sidebar:
  label: "OutputSpec"
  hidden: true
---

<a id="Vercel_AI_OutputSpec"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public abstract class OutputSpec
```


Structured output selector. Maps to `Output`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [OutputSpec&lt;TOutput&gt;](/api/Vercel.AI/OutputSpec-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L568)

## Methods

<a id="Vercel_AI_OutputSpec_Array_"></a>
<a id="Vercel_AI_OutputSpec_Array_System_String_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_System_String_"></a>

### Array(string, int?, int?, string?, string?)


```csharp
public static OutputSpec Array(string elementJsonSchema, int? minItems = null, int? maxItems = null, string? name = null, string? description = null)
```


Array output with the element schema as a string. Maps to `Output.array`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `elementJsonSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `minItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OutputSpec](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | &lt;code class="paramref"&gt;minItems&lt;/code&gt; or &lt;code class="paramref"&gt;maxItems&lt;/code&gt; is negative, or &lt;code class="paramref"&gt;minItems&lt;/code&gt; is greater than &lt;code class="paramref"&gt;maxItems&lt;/code&gt;. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L615)

<a id="Vercel_AI_OutputSpec_Array_System_Text_Json_JsonElement_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_System_String_"></a>

### Array(JsonElement, int?, int?, string?, string?)


```csharp
public static OutputSpec Array(JsonElement elementSchema, int? minItems = null, int? maxItems = null, string? name = null, string? description = null)
```


Array output. Each element is validated against &lt;code class="paramref"&gt;elementSchema&lt;/code&gt;. The model answers with
`{ "elements": [...] }` and the output is the array of elements. Maps to `Output.array`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `elementSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `minItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OutputSpec](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | &lt;code class="paramref"&gt;minItems&lt;/code&gt; or &lt;code class="paramref"&gt;maxItems&lt;/code&gt; is negative, or &lt;code class="paramref"&gt;minItems&lt;/code&gt; is greater than &lt;code class="paramref"&gt;maxItems&lt;/code&gt;. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L602)

<a id="Vercel_AI_OutputSpec_Array__1_System_String_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_System_String_"></a>

### `Array<TElement>(string, int?, int?, string?, string?)`


```csharp
public static OutputSpec<IReadOnlyList<TElement>> Array<TElement>(string elementJsonSchema, int? minItems = null, int? maxItems = null, string? name = null, string? description = null)
```


Array output with the element schema as a string, deserialized to &lt;code class="typeparamref"&gt;TElement&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `TElement` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `elementJsonSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `minItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `OutputSpec&lt;IReadOnlyList&lt;TElement&gt;&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L684)

<a id="Vercel_AI_OutputSpec_Array__1_System_Text_Json_JsonElement_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_System_String_"></a>

### `Array<TElement>(JsonElement, int?, int?, string?, string?)`


```csharp
public static OutputSpec<IReadOnlyList<TElement>> Array<TElement>(JsonElement elementSchema, int? minItems = null, int? maxItems = null, string? name = null, string? description = null)
```


Array output whose elements are deserialized to &lt;code class="typeparamref"&gt;TElement&lt;/code&gt;. Maps to `Output.array`
with an inferred element type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TElement` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `elementSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `minItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `OutputSpec&lt;IReadOnlyList&lt;TElement&gt;&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L670)

<a id="Vercel_AI_OutputSpec_Choice_"></a>
<a id="Vercel_AI_OutputSpec_Choice_System_Collections_Generic_IReadOnlyList_System_String__System_String_System_String_"></a>

### `Choice(IReadOnlyList<string>, string?, string?)`


```csharp
public static OutputSpec Choice(IReadOnlyList<string> options, string? name = null, string? description = null)
```


Choice output. The model answers with `{ "result": option }` and the output is the option. Maps to `Output.choice`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OutputSpec](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L628)

<a id="Vercel_AI_OutputSpec_Json_"></a>
<a id="Vercel_AI_OutputSpec_Json_System_String_System_String_"></a>

### Json(string?, string?)


```csharp
public static OutputSpec Json(string? name = null, string? description = null)
```


Unstructured JSON output. The output is the parsed JSON value. Maps to `Output.json`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OutputSpec](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L638)

<a id="Vercel_AI_OutputSpec_Object_"></a>
<a id="Vercel_AI_OutputSpec_Object_System_String_System_String_System_String_"></a>

### Object(string, string?, string?)


```csharp
public static OutputSpec Object(string jsonSchema, string? name = null, string? description = null)
```


JSON object output validated against a schema string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `jsonSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OutputSpec](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L587)

<a id="Vercel_AI_OutputSpec_Object_System_Text_Json_JsonElement_System_String_System_String_"></a>

### Object(JsonElement, string?, string?)


```csharp
public static OutputSpec Object(JsonElement schema, string? name = null, string? description = null)
```


JSON object output validated against &lt;code class="paramref"&gt;schema&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OutputSpec](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L577)

<a id="Vercel_AI_OutputSpec_Object__1_System_String_System_String_System_String_"></a>

### `Object<TOutput>(string, string?, string?)`


```csharp
public static OutputSpec<TOutput> Object<TOutput>(string jsonSchema, string? name = null, string? description = null)
```


JSON object output from a schema string, deserialized to &lt;code class="typeparamref"&gt;TOutput&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `jsonSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `OutputSpec&lt;TOutput&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L657)

<a id="Vercel_AI_OutputSpec_Object__1_System_Text_Json_JsonElement_System_String_System_String_"></a>

### `Object<TOutput>(JsonElement, string?, string?)`


```csharp
public static OutputSpec<TOutput> Object<TOutput>(JsonElement schema, string? name = null, string? description = null)
```


JSON object output deserialized to &lt;code class="typeparamref"&gt;TOutput&lt;/code&gt;. Maps to `Output.object` with an
inferred output type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `OutputSpec&lt;TOutput&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L647)

<a id="Vercel_AI_OutputSpec_Text_"></a>
<a id="Vercel_AI_OutputSpec_Text"></a>

### Text()


```csharp
public static OutputSpec Text()
```


Plain text output.

**Returns:** [OutputSpec](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L571)
