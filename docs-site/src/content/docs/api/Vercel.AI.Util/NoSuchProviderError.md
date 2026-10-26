---
title: "NoSuchProviderError"
description: "A registry has no provider with the requested id. Maps to NoSuchProviderError."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoSuchProviderError"
  hidden: true
---

<a id="Vercel_AI_Util_NoSuchProviderError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class NoSuchProviderError : NoSuchModelError, ISerializable, IErrorMarkers
```


A registry has no provider with the requested id. Maps to `NoSuchProviderError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/) → [NoSuchModelError](/api/Vercel.AI.Util/NoSuchModelError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchProviderError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_NoSuchProviderError__ctor_"></a>
<a id="Vercel_AI_Util_NoSuchProviderError__ctor_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_String_"></a>

### `NoSuchProviderError(string, string, string, IReadOnlyList<string>, string?)`


```csharp
public NoSuchProviderError(string modelId, string modelType, string providerId, IReadOnlyList<string> availableProviders, string? message = null)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `availableProviders` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchProviderError.cs#L14)

## Fields

<a id="Vercel_AI_Util_NoSuchProviderError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_NoSuchProviderError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_NoSuchProviderError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchProviderError.cs#L11)

## Properties

<a id="Vercel_AI_Util_NoSuchProviderError_AvailableProviders_"></a>
<a id="Vercel_AI_Util_NoSuchProviderError_AvailableProviders"></a>

### AvailableProviders


```csharp
public IReadOnlyList<string> AvailableProviders { get; }
```


Registered provider ids.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchProviderError.cs#L30)

<a id="Vercel_AI_Util_NoSuchProviderError_ProviderId_"></a>
<a id="Vercel_AI_Util_NoSuchProviderError_ProviderId"></a>

### ProviderId


```csharp
public string ProviderId { get; }
```


Requested provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchProviderError.cs#L27)

## Methods

<a id="Vercel_AI_Util_NoSuchProviderError_IsInstance_"></a>
<a id="Vercel_AI_Util_NoSuchProviderError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchProviderError.cs#L33)
