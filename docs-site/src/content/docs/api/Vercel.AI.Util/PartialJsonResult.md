---
title: "PartialJsonResult"
description: "Outcome of Parse(string)."
editUrl: false
prev: false
next: false
sidebar:
  label: "PartialJsonResult"
  hidden: true
---

<a id="Vercel_AI_Util_PartialJsonResult"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class PartialJsonResult
```


Outcome of [Parse(string)](/api/Vercel.AI.Util/PartialJson/#Vercel_AI_Util_PartialJson_Parse_System_String_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L10)

## Constructors

<a id="Vercel_AI_Util_PartialJsonResult__ctor_"></a>
<a id="Vercel_AI_Util_PartialJsonResult__ctor_System_Text_Json_Nodes_JsonNode_System_String_"></a>

### PartialJsonResult(JsonNode?, string)


```csharp
public PartialJsonResult(JsonNode? value, string state)
```


Creates a parse outcome.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `state` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L13)

## Properties

<a id="Vercel_AI_Util_PartialJsonResult_State_"></a>
<a id="Vercel_AI_Util_PartialJsonResult_State"></a>

### State


```csharp
public string State { get; }
```


`undefined-input`, `successful-parse`, `repaired-parse`, or `failed-parse`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L25)

<a id="Vercel_AI_Util_PartialJsonResult_Value_"></a>
<a id="Vercel_AI_Util_PartialJsonResult_Value"></a>

### Value


```csharp
public JsonNode? Value { get; }
```


Parsed value, or `null` when parsing failed or the input was undefined.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L20)
