---
title: "ExtractionUnsupportedFieldTypeError"
description: "An extraction model does not support the type of a requested field."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionUnsupportedFieldTypeError"
  hidden: true
---

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ExtractionUnsupportedFieldTypeError : AiSdkError, ISerializable, IErrorMarkers
```


An extraction model does not support the type of a requested field.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError__ctor_"></a>
<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError__ctor_System_String_System_String_System_String_System_String_System_String_"></a>

### ExtractionUnsupportedFieldTypeError(string, string, string, string, string?)


```csharp
public ExtractionUnsupportedFieldTypeError(string fieldId, string fieldType, string provider, string modelId, string? message = null)
```


Creates the error for the unsupported field.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fieldId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fieldType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L14)

## Fields

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_ExtractionUnsupportedFieldTypeError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L11)

## Properties

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_FieldId_"></a>
<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_FieldId"></a>

### FieldId


```csharp
public string FieldId { get; }
```


Field identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L32)

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_FieldType_"></a>
<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_FieldType"></a>

### FieldType


```csharp
public string FieldType { get; }
```


Field type the model rejected.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L35)

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_ModelId_"></a>
<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Extraction model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L41)

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_Provider_"></a>
<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Extraction provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L38)

## Methods

<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_IsInstance_"></a>
<a id="Vercel_AI_Util_ExtractionUnsupportedFieldTypeError_IsInstance_System_Object_"></a>

### IsInstance(object?)


```csharp
public static bool IsInstance(object? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; carries this error's marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionUnsupportedFieldTypeError.cs#L44)
