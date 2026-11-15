---
title: "ExtractionRefusalError"
description: "An extraction model declined to extract one or more fields."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionRefusalError"
  hidden: true
---

<a id="Vercel_AI_Util_ExtractionRefusalError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ExtractionRefusalError : AiSdkError, ISerializable, IErrorMarkers
```


An extraction model declined to extract one or more fields.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L11)

## Constructors

<a id="Vercel_AI_Util_ExtractionRefusalError__ctor_"></a>
<a id="Vercel_AI_Util_ExtractionRefusalError__ctor_System_Collections_Generic_IReadOnlyList_System_String__System_String_System_String_System_String_"></a>

### `ExtractionRefusalError(IReadOnlyList<string>, string, string, string?)`


```csharp
public ExtractionRefusalError(IReadOnlyList<string> fieldIds, string provider, string modelId, string? message = null)
```


Creates the error for the refused fields.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fieldIds` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L19)

## Fields

<a id="Vercel_AI_Util_ExtractionRefusalError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_ExtractionRefusalError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_ExtractionRefusalError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L14)

## Properties

<a id="Vercel_AI_Util_ExtractionRefusalError_FieldIds_"></a>
<a id="Vercel_AI_Util_ExtractionRefusalError_FieldIds"></a>

### FieldIds


```csharp
public IReadOnlyList<string> FieldIds { get; }
```


Refused field ids, in field order.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L35)

<a id="Vercel_AI_Util_ExtractionRefusalError_ModelId_"></a>
<a id="Vercel_AI_Util_ExtractionRefusalError_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Extraction model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L41)

<a id="Vercel_AI_Util_ExtractionRefusalError_Provider_"></a>
<a id="Vercel_AI_Util_ExtractionRefusalError_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Extraction provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L38)

## Methods

<a id="Vercel_AI_Util_ExtractionRefusalError_IsInstance_"></a>
<a id="Vercel_AI_Util_ExtractionRefusalError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ExtractionRefusalError.cs#L44)
