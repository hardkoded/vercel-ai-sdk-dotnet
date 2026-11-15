---
title: "NoObjectGeneratedException"
description: "Thrown when a model response cannot produce the requested object. Maps to NoObjectGeneratedError."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoObjectGeneratedException"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class NoObjectGeneratedException : AiSdkError, ISerializable, IErrorMarkers
```


Thrown when a model response cannot produce the requested object. Maps to `NoObjectGeneratedError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L14)

## Constructors

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException__ctor_"></a>
<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException__ctor_System_String_System_Exception_System_String_Vercel_AI_StepResponse_Vercel_AI_Provider_LanguageModelUsage_Vercel_AI_Provider_FinishReason_"></a>

### NoObjectGeneratedException(string, Exception?, string?, StepResponse, LanguageModelUsage, FinishReason)


```csharp
public NoObjectGeneratedException(string message, Exception? cause, string? text, StepResponse response, LanguageModelUsage usage, FinishReason finishReason)
```


Creates the exception and preserves the failed generation details.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `response` | [StepResponse](/api/Vercel.AI/StepResponse/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L20)

## Fields

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_NoObjectGeneratedError"
```


Marker for [IsInstance(object)](#Vercel_AI_GenerateObject_NoObjectGeneratedException_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L17)

## Properties

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_FinishReason_"></a>
<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Why the model finished generating.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L46)

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_Response_"></a>
<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_Response"></a>

### Response


```csharp
public StepResponse Response { get; }
```


Response metadata from the model call.

**Value:** [StepResponse](/api/Vercel.AI/StepResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L40)

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_Text_"></a>
<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_Text"></a>

### Text


```csharp
public string? Text { get; }
```


The selected text that failed to parse or validate.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L37)

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_Usage_"></a>
<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Token usage from the model call.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L43)

## Methods

<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_IsInstance_"></a>
<a id="Vercel_AI_GenerateObject_NoObjectGeneratedException_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L49)
