---
title: "NoSuchModelError"
description: "A provider has no model with the requested id. Maps to NoSuchModelError."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoSuchModelError"
  hidden: true
---

<a id="Vercel_AI_Util_NoSuchModelError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public class NoSuchModelError : AiSdkError, ISerializable, IErrorMarkers
```


A provider has no model with the requested id. Maps to `NoSuchModelError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

**Derived:** [NoSuchProviderError](/api/Vercel.AI.Util/NoSuchProviderError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_NoSuchModelError__ctor_"></a>
<a id="Vercel_AI_Util_NoSuchModelError__ctor_System_String_System_String_System_String_"></a>

### NoSuchModelError(string, string, string?)


```csharp
public NoSuchModelError(string modelId, string modelType, string? message = null)
```


Creates the error. &lt;code class="paramref"&gt;modelType&lt;/code&gt; is a name such as `languageModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L14)

<a id="Vercel_AI_Util_NoSuchModelError__ctor_System_String_System_String_System_String_System_String_"></a>

### NoSuchModelError(string, string, string, string)


```csharp
protected NoSuchModelError(string errorName, string modelId, string modelType, string message)
```


Creates the error with a subclass error name.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `errorName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L20)

## Fields

<a id="Vercel_AI_Util_NoSuchModelError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_NoSuchModelError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_NoSuchModelError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L11)

## Properties

<a id="Vercel_AI_Util_NoSuchModelError_ModelId_"></a>
<a id="Vercel_AI_Util_NoSuchModelError_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Requested model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L29)

<a id="Vercel_AI_Util_NoSuchModelError_ModelType_"></a>
<a id="Vercel_AI_Util_NoSuchModelError_ModelType"></a>

### ModelType


```csharp
public string ModelType { get; }
```


Requested model type, such as `languageModel`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L32)

## Methods

<a id="Vercel_AI_Util_NoSuchModelError_IsInstance_"></a>
<a id="Vercel_AI_Util_NoSuchModelError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchModelError.cs#L35)
