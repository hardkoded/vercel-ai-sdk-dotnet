---
title: "SerializationError"
description: "A value could not be serialized. Maps to SerializationError."
editUrl: false
prev: false
next: false
sidebar:
  label: "SerializationError"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SerializationError"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SerializationError : AiSdkError, ISerializable, IErrorMarkers
```


A value could not be serialized. Maps to `SerializationError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L100)

## Constructors

<a id="Vercel_AI_ProviderUtils_SerializationError__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_SerializationError__ctor_System_String_System_Exception_"></a>

### SerializationError(string?, Exception?)


```csharp
public SerializationError(string? message = null, Exception? cause = null)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L106)

## Fields

<a id="Vercel_AI_ProviderUtils_SerializationError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_SerializationError"
```


Marker for [IsInstance(object)](#Vercel_AI_ProviderUtils_SerializationError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L103)

## Methods

<a id="Vercel_AI_ProviderUtils_SerializationError_IsInstance_"></a>
<a id="Vercel_AI_ProviderUtils_SerializationError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L113)
