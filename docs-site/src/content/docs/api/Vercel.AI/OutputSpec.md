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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L485)

## Methods

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L500)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L494)

<a id="Vercel_AI_OutputSpec_Text_"></a>
<a id="Vercel_AI_OutputSpec_Text"></a>

### Text()


```csharp
public static OutputSpec Text()
```


Plain text output.

**Returns:** [OutputSpec](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L488)
