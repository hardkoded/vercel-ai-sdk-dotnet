---
title: "ModelCapability"
description: "Support declared by a provider's model catalog. Upstream ModelCapability (boolean | 'unknown' | { condition: string }). Unknown is distinct from unsupported...."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelCapability"
  hidden: true
---

<a id="Vercel_AI_ModelCapability"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ModelCapability
```


Support declared by a provider's model catalog. Upstream `ModelCapability`
(`boolean | 'unknown' | { condition: string }`). Unknown is distinct from unsupported.
A condition describes when support is available; it is documentation, not an expression to execute.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L12)

## Fields

<a id="Vercel_AI_ModelCapability_Supported"></a>

### Supported


```csharp
public static readonly ModelCapability Supported
```


Supported.

**Value:** [ModelCapability](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L15)

<a id="Vercel_AI_ModelCapability_Unknown"></a>

### Unknown


```csharp
public static readonly ModelCapability Unknown
```


Support is not declared. Upstream `'unknown'`.

**Value:** [ModelCapability](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L21)

<a id="Vercel_AI_ModelCapability_Unsupported"></a>

### Unsupported


```csharp
public static readonly ModelCapability Unsupported
```


Not supported.

**Value:** [ModelCapability](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L18)

## Properties

<a id="Vercel_AI_ModelCapability_Condition_"></a>
<a id="Vercel_AI_ModelCapability_Condition"></a>

### Condition


```csharp
public string? Condition { get; }
```


When support is available, for the `{ condition }` form. Null otherwise.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L37)

<a id="Vercel_AI_ModelCapability_IsSupported_"></a>
<a id="Vercel_AI_ModelCapability_IsSupported"></a>

### IsSupported


```csharp
public bool IsSupported { get; }
```


True when support is declared as `true`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L31)

<a id="Vercel_AI_ModelCapability_IsUnknown_"></a>
<a id="Vercel_AI_ModelCapability_IsUnknown"></a>

### IsUnknown


```csharp
public bool IsUnknown { get; }
```


True when support is declared as `'unknown'`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L34)

## Methods

<a id="Vercel_AI_ModelCapability_From_"></a>
<a id="Vercel_AI_ModelCapability_From_System_Boolean_"></a>

### From(bool)


```csharp
public static ModelCapability From(bool supported)
```


Maps a boolean to [Supported](#Vercel_AI_ModelCapability_Supported) or [Unsupported](#Vercel_AI_ModelCapability_Unsupported).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `supported` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [ModelCapability](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L40)

<a id="Vercel_AI_ModelCapability_When_"></a>
<a id="Vercel_AI_ModelCapability_When_System_String_"></a>

### When(string)


```csharp
public static ModelCapability When(string condition)
```


The `{ condition }` form.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `condition` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ModelCapability](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L43)
