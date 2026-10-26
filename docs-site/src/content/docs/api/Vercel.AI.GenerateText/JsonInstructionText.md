---
title: "JsonInstructionText"
description: "An optional string that can be omitted, null, or set. Distinguishes a missing argument from null."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonInstructionText"
  hidden: true
---

<a id="Vercel_AI_GenerateText_JsonInstructionText"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public readonly struct JsonInstructionText
```


An optional string that can be omitted, null, or set. Distinguishes a missing argument from null.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L10)

## Properties

<a id="Vercel_AI_GenerateText_JsonInstructionText_IsSet_"></a>
<a id="Vercel_AI_GenerateText_JsonInstructionText_IsSet"></a>

### IsSet


```csharp
public bool IsSet { get; }
```


True when the caller passed a value, including null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L19)

<a id="Vercel_AI_GenerateText_JsonInstructionText_Null_"></a>
<a id="Vercel_AI_GenerateText_JsonInstructionText_Null"></a>

### Null


```csharp
public static JsonInstructionText Null { get; }
```


The argument was passed as null.

**Value:** [JsonInstructionText](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L28)

<a id="Vercel_AI_GenerateText_JsonInstructionText_Unset_"></a>
<a id="Vercel_AI_GenerateText_JsonInstructionText_Unset"></a>

### Unset


```csharp
public static JsonInstructionText Unset { get; }
```


The argument was omitted.

**Value:** [JsonInstructionText](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L25)

<a id="Vercel_AI_GenerateText_JsonInstructionText_Value_"></a>
<a id="Vercel_AI_GenerateText_JsonInstructionText_Value"></a>

### Value


```csharp
public string? Value { get; }
```


The text, when [IsSet](#Vercel_AI_GenerateText_JsonInstructionText_IsSet) is true.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L22)

## Methods

<a id="Vercel_AI_GenerateText_JsonInstructionText_From_"></a>
<a id="Vercel_AI_GenerateText_JsonInstructionText_From_System_String_"></a>

### From(string?)


```csharp
public static JsonInstructionText From(string? value)
```


The argument was passed as &lt;code class="paramref"&gt;value&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonInstructionText](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L31)
