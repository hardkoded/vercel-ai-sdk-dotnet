---
title: "AiSdkError"
description: "Base error for upstream AI SDK error types. Marker checks work without a shared assembly."
editUrl: false
prev: false
next: false
sidebar:
  label: "AiSdkError"
  hidden: true
---

<a id="Vercel_AI_Util_AiSdkError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public class AiSdkError : Exception, ISerializable, IErrorMarkers
```


Base error for upstream AI SDK error types. Marker checks work without a shared assembly.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

**Derived:** [NoSuchProviderReferenceError](/api/Vercel.AI.ProviderUtils/NoSuchProviderReferenceError/), [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/), [ApiCallError](/api/Vercel.AI.Util/ApiCallError/), [DecisionRefusalError](/api/Vercel.AI.Util/DecisionRefusalError/), [DecisionUnsupportedQuestionTypeError](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/), [DownloadError](/api/Vercel.AI.Util/DownloadError/), [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/), [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/), [NoSuchModelError](/api/Vercel.AI.Util/NoSuchModelError/), [NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/), [RetryError](/api/Vercel.AI.Util/RetryError/), [StreamProviderError](/api/Vercel.AI.Util/StreamProviderError/), [ToolCallRepairError](/api/Vercel.AI.Util/ToolCallRepairError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L52)

## Constructors

<a id="Vercel_AI_Util_AiSdkError__ctor_"></a>
<a id="Vercel_AI_Util_AiSdkError__ctor_System_String_System_String_System_Exception_"></a>

### AiSdkError(string?, string, Exception?)


```csharp
public AiSdkError(string? name, string message, Exception? cause = null)
```


Creates an error with the upstream &lt;code class="paramref"&gt;name&lt;/code&gt; and &lt;code class="paramref"&gt;message&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L60)

## Fields

<a id="Vercel_AI_Util_AiSdkError_Marker"></a>

### Marker


```csharp
public const string Marker = "vercel.ai.error"
```


Marker present on every AI SDK error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L55)

## Properties

<a id="Vercel_AI_Util_AiSdkError_Cause_"></a>
<a id="Vercel_AI_Util_AiSdkError_Cause"></a>

### Cause


```csharp
public Exception? Cause { get; }
```


Underlying cause, when one was supplied.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L72)

<a id="Vercel_AI_Util_AiSdkError_ErrorName_"></a>
<a id="Vercel_AI_Util_AiSdkError_ErrorName"></a>

### ErrorName


```csharp
public string ErrorName { get; }
```


Upstream error name, such as `AI_APICallError`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L69)

## Methods

<a id="Vercel_AI_Util_AiSdkError_HasMarker_"></a>
<a id="Vercel_AI_Util_AiSdkError_HasMarker_System_Object_System_String_"></a>

### HasMarker(object?, string?)


```csharp
public static bool HasMarker(object? error, string? marker)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; has &lt;code class="paramref"&gt;marker&lt;/code&gt; set to `true`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `marker` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L97)

<a id="Vercel_AI_Util_AiSdkError_HasMarker_System_String_"></a>

### HasMarker(string)


```csharp
public bool HasMarker(string marker)
```


Returns whether &lt;code class="paramref"&gt;marker&lt;/code&gt; is set to `true`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `marker` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L84)

<a id="Vercel_AI_Util_AiSdkError_IsInstance_"></a>
<a id="Vercel_AI_Util_AiSdkError_IsInstance_System_Object_"></a>

### IsInstance(object?)


```csharp
public static bool IsInstance(object? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; carries the base AI SDK marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L91)

<a id="Vercel_AI_Util_AiSdkError_SetMarker_"></a>
<a id="Vercel_AI_Util_AiSdkError_SetMarker_System_String_System_Boolean_"></a>

### SetMarker(string, bool)


```csharp
protected void SetMarker(string marker, bool value)
```


Records a marker used by `String)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `marker` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L75)
