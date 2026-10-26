---
title: "InvalidArgumentError"
description: "An argument failed validation. Message format matches the TypeScript SDK."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidArgumentError"
  hidden: true
---

<a id="Vercel_AI_Util_InvalidArgumentError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class InvalidArgumentError : AiSdkError, ISerializable, IErrorMarkers
```


An argument failed validation. Message format matches the TypeScript SDK.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L105)

## Constructors

<a id="Vercel_AI_Util_InvalidArgumentError__ctor_"></a>
<a id="Vercel_AI_Util_InvalidArgumentError__ctor_System_String_System_Object_System_String_"></a>

### InvalidArgumentError(string, object?, string)


```csharp
public InvalidArgumentError(string parameter, object? value, string message)
```


Creates the error for &lt;code class="paramref"&gt;parameter&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parameter` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L111)

## Fields

<a id="Vercel_AI_Util_InvalidArgumentError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_InvalidArgumentError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_InvalidArgumentError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L108)

## Properties

<a id="Vercel_AI_Util_InvalidArgumentError_Parameter_"></a>
<a id="Vercel_AI_Util_InvalidArgumentError_Parameter"></a>

### Parameter


```csharp
public string Parameter { get; }
```


Parameter that failed validation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L120)

<a id="Vercel_AI_Util_InvalidArgumentError_Value_"></a>
<a id="Vercel_AI_Util_InvalidArgumentError_Value"></a>

### Value


```csharp
public object? Value { get; }
```


Rejected value.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L123)

## Methods

<a id="Vercel_AI_Util_InvalidArgumentError_IsInstance_"></a>
<a id="Vercel_AI_Util_InvalidArgumentError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L126)
