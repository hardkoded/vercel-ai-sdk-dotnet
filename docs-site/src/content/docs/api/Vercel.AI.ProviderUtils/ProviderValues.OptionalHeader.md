---
title: "ProviderValues.OptionalHeader"
description: "Header values that may be omitted."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderValues.OptionalHeader"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public readonly struct ProviderValues.OptionalHeader
```


Header values that may be omitted.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L142)

## Constructors

<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader__ctor_System_String_"></a>

### OptionalHeader(string?)


```csharp
public OptionalHeader(string? value)
```


Creates a header value. Null means the header is omitted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L145)

## Properties

<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader_HasValue_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader_HasValue"></a>

### HasValue


```csharp
public bool HasValue { get; }
```


True when the caller supplied a value, including null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L158)

<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader_Omitted_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader_Omitted"></a>

### Omitted


```csharp
public static ProviderValues.OptionalHeader Omitted { get; }
```


An omitted header.

**Value:** [ProviderValues.OptionalHeader](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L152)

<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader_Value_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_OptionalHeader_Value"></a>

### Value


```csharp
public string? Value { get; }
```


Header value.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L161)
