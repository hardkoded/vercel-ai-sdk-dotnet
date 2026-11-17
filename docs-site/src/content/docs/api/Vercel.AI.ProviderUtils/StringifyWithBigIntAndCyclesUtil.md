---
title: "StringifyWithBigIntAndCyclesUtil"
description: "Maps to stringify-with-bigint-and-cycles.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "StringifyWithBigIntAndCyclesUtil"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_StringifyWithBigIntAndCyclesUtil"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class StringifyWithBigIntAndCyclesUtil
```


Maps to `stringify-with-bigint-and-cycles.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/StringifyWithBigIntAndCycles.cs#L14)

## Methods

<a id="Vercel_AI_ProviderUtils_StringifyWithBigIntAndCyclesUtil_StringifyWithBigIntAndCycles_"></a>
<a id="Vercel_AI_ProviderUtils_StringifyWithBigIntAndCyclesUtil_StringifyWithBigIntAndCycles_System_Object_"></a>

### StringifyWithBigIntAndCycles(object?)


```csharp
public static string StringifyWithBigIntAndCycles(object? value)
```


Serializes a value as JSON, representing [BigInteger](https://learn.microsoft.com/dotnet/api/system.numerics.biginteger) values as decimal strings and
ancestor cycles as `"[Circular]"`. Reused objects are serialized each time. Maps to `stringifyWithBigIntAndCycles`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/StringifyWithBigIntAndCycles.cs#L25)
