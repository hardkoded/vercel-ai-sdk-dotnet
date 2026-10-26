---
title: "NoSuchProviderReferenceError"
description: "A provider reference has no entry for the requested provider. Maps to NoSuchProviderReferenceError."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoSuchProviderReferenceError"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class NoSuchProviderReferenceError : AiSdkError, ISerializable, IErrorMarkers
```


A provider reference has no entry for the requested provider. Maps to `NoSuchProviderReferenceError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L46)

## Constructors

<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `NoSuchProviderReferenceError(string, IReadOnlyDictionary<string, string>, string?)`


```csharp
public NoSuchProviderReferenceError(string provider, IReadOnlyDictionary<string, string> reference, string? message = null)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L52)

## Fields

<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_NoSuchProviderReferenceError"
```


Marker for [IsInstance(object)](#Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L49)

## Properties

<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_Provider_"></a>
<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Requested provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L63)

<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_Reference_"></a>
<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_Reference"></a>

### Reference


```csharp
public IReadOnlyDictionary<string, string> Reference { get; }
```


The reference that was searched.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L66)

## Methods

<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_IsInstance_"></a>
<a id="Vercel_AI_ProviderUtils_NoSuchProviderReferenceError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L69)
