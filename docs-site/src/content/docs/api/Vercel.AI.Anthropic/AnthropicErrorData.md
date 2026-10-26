---
title: "AnthropicErrorData"
description: "Parsed Anthropic error body. Extra fields such as details are dropped."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicErrorData"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicErrorData"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicErrorData
```


Parsed Anthropic error body. Extra fields such as `details` are dropped.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L11)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicErrorData__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData__ctor_System_Boolean_System_String_System_String_System_String_System_String_"></a>

### AnthropicErrorData(bool, string?, string?, string?, string)


```csharp
public AnthropicErrorData(bool success, string? type, string? errorType, string? message, string rawValue)
```


Creates a parsed error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `success` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `errorType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `rawValue` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L14)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicErrorData_ErrorType_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_ErrorType"></a>

### ErrorType


```csharp
public string? ErrorType { get; }
```


Error type, such as `overloaded_error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L30)

<a id="Vercel_AI_Anthropic_AnthropicErrorData_Message_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_Message"></a>

### Message


```csharp
public string? Message { get; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L33)

<a id="Vercel_AI_Anthropic_AnthropicErrorData_RawValue_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_RawValue"></a>

### RawValue


```csharp
public string RawValue { get; }
```


Original JSON.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L36)

<a id="Vercel_AI_Anthropic_AnthropicErrorData_Success_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when the body matches `{ type: error, error: { type, message } }`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L24)

<a id="Vercel_AI_Anthropic_AnthropicErrorData_Type_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Top-level type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L27)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicErrorData_Parse_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_Parse_System_String_"></a>

### Parse(string)


```csharp
public static AnthropicErrorData Parse(string json)
```


Parses an error payload. A matching object succeeds even when extra properties are present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AnthropicErrorData](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L39)

<a id="Vercel_AI_Anthropic_AnthropicErrorData_Value_"></a>
<a id="Vercel_AI_Anthropic_AnthropicErrorData_Value"></a>

### Value()


```csharp
public JsonObject? Value()
```


The validated value with only `type` and `error.type`/`error.message`.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L64)
