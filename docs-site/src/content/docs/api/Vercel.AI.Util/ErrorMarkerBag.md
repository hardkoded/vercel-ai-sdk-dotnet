---
title: "ErrorMarkerBag"
description: "Plain marker bag used to recognize an error shape copied across packages."
editUrl: false
prev: false
next: false
sidebar:
  label: "ErrorMarkerBag"
  hidden: true
---

<a id="Vercel_AI_Util_ErrorMarkerBag"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ErrorMarkerBag : IErrorMarkers
```


Plain marker bag used to recognize an error shape copied across packages.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L18)

## Constructors

<a id="Vercel_AI_Util_ErrorMarkerBag__ctor_"></a>
<a id="Vercel_AI_Util_ErrorMarkerBag__ctor_System_Collections_Generic_KeyValuePair_System_String_System_Boolean____"></a>

### `ErrorMarkerBag(params KeyValuePair<string, bool>[])`


```csharp
public ErrorMarkerBag(params KeyValuePair<string, bool>[] markers)
```


Creates a bag with the given marker values.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `markers` | [KeyValuePair&lt;string, bool&gt;\[\]](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L23)

<a id="Vercel_AI_Util_ErrorMarkerBag__ctor_System_String_System_Boolean_"></a>

### ErrorMarkerBag(string, bool)


```csharp
public ErrorMarkerBag(string marker, bool value)
```


Creates a bag with one marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `marker` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L36)

## Methods

<a id="Vercel_AI_Util_ErrorMarkerBag_HasMarker_"></a>
<a id="Vercel_AI_Util_ErrorMarkerBag_HasMarker_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AiSdkError.cs#L42)
