---
title: "JsonInstructions"
description: "Appends a JSON schema instruction to a prompt or to the first system message."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonInstructions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonInstructions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonInstructions
```


Appends a JSON schema instruction to a prompt or to the first system message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L515)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonInstructions_Inject_"></a>
<a id="Vercel_AI_ProviderUtils_JsonInstructions_Inject_System_String_System_Text_Json_Nodes_JsonNode_System_String_System_String_System_Boolean_System_Boolean_"></a>

### Inject(string?, JsonNode?, string?, string?, bool, bool)


```csharp
public static string Inject(string? prompt, JsonNode? schema, string? schemaPrefix = null, string? schemaSuffix = null, bool prefixSpecified = false, bool suffixSpecified = false)
```


Joins an optional prompt with a JSON schema instruction.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `schemaPrefix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `schemaSuffix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `prefixSpecified` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `suffixSpecified` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L522)

<a id="Vercel_AI_ProviderUtils_JsonInstructions_InjectIntoMessages_"></a>
<a id="Vercel_AI_ProviderUtils_JsonInstructions_InjectIntoMessages_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Text_Json_Nodes_JsonNode_System_String_System_String_System_Boolean_System_Boolean_"></a>

### `InjectIntoMessages(IReadOnlyList<ModelMessage>, JsonNode?, string?, string?, bool, bool)`


```csharp
public static IReadOnlyList<ModelMessage> InjectIntoMessages(IReadOnlyList<ModelMessage> messages, JsonNode? schema = null, string? schemaPrefix = null, string? schemaSuffix = null, bool prefixSpecified = false, bool suffixSpecified = false)
```


Writes the instruction into the first system message, or inserts one.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `schemaPrefix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `schemaSuffix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `prefixSpecified` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `suffixSpecified` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L552)
