---
title: "DecisionInput"
description: "Content for decision instructions and criteria descriptions: a string, a JSON object, or a JSON array. Maps to DecisionModelV4Input."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionInput"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionInput"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionInput
```


Content for decision instructions and criteria descriptions: a string, a JSON object, or a JSON array.
Maps to `DecisionModelV4Input`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L14)

## Properties

<a id="Vercel_AI_Operations_DecisionInput_Value_"></a>
<a id="Vercel_AI_Operations_DecisionInput_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


The JSON value. It is a string, an object, or an array.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L22)

## Methods

<a id="Vercel_AI_Operations_DecisionInput_From_"></a>
<a id="Vercel_AI_Operations_DecisionInput_From_System_String_"></a>

### From(string)


```csharp
public static DecisionInput From(string text)
```


Creates an input from a string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L25)

<a id="Vercel_AI_Operations_DecisionInput_FromJson_"></a>
<a id="Vercel_AI_Operations_DecisionInput_FromJson_System_Text_Json_JsonElement_"></a>

### FromJson(JsonElement)


```csharp
public static DecisionInput FromJson(JsonElement value)
```


Creates an input from a JSON string, object, or array. Other JSON values throw [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [DecisionInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L37)

<a id="Vercel_AI_Operations_DecisionInput_Parse_"></a>
<a id="Vercel_AI_Operations_DecisionInput_Parse_System_String_"></a>

### Parse(string)


```csharp
public static DecisionInput Parse(string json)
```


Parses JSON text into an input. The text must be a JSON string, object, or array.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L48)

<a id="Vercel_AI_Operations_DecisionInput_ToString_"></a>
<a id="Vercel_AI_Operations_DecisionInput_ToString"></a>

### ToString()


```csharp
public override string ToString()
```


The string itself, or the JSON text of an object or array.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L61)

## Operators

<a id="Vercel_AI_Operations_DecisionInput_op_Implicit_"></a>
<a id="Vercel_AI_Operations_DecisionInput_op_Implicit_System_String__Vercel_AI_Operations_DecisionInput"></a>

### implicit operator DecisionInput(string)


```csharp
public static implicit operator DecisionInput(string text)
```


Converts a string to an input.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L55)
