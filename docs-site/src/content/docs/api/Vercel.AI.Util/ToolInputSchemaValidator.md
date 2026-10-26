---
title: "ToolInputSchemaValidator"
description: "Validates a tool call input against the JSON Schema of a tool. Covers the keywords tool schemas use: type, enum, const, properties, required, additionalPrope..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputSchemaValidator"
  hidden: true
---

<a id="Vercel_AI_Util_ToolInputSchemaValidator"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class ToolInputSchemaValidator
```


Validates a tool call input against the JSON Schema of a tool. Covers the keywords tool schemas use:
`type`, `enum`, `const`, `properties`, `required`, `additionalProperties`, `items`,
`minItems`, `maxItems`, `minLength`, `maxLength`, `minimum`, `maximum`,
`anyOf`, `oneOf` and `allOf`. Other keywords are ignored.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolInputSchemaValidator.cs#L16)

## Methods

<a id="Vercel_AI_Util_ToolInputSchemaValidator_Validate_"></a>
<a id="Vercel_AI_Util_ToolInputSchemaValidator_Validate_System_Text_Json_JsonElement_System_Text_Json_JsonElement_"></a>

### Validate(JsonElement, JsonElement)


```csharp
public static IReadOnlyList<string> Validate(JsonElement schema, JsonElement value)
```


Returns the validation errors of &lt;code class="paramref"&gt;value&lt;/code&gt;, or an empty list when it is valid.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolInputSchemaValidator.cs#L19)
