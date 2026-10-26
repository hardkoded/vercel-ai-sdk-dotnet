---
title: "ImageResponseInfo"
description: "Response metadata of an image call. Maps to response of doGenerate on an image model."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageResponseInfo"
  hidden: true
---

<a id="Vercel_AI_Provider_ImageResponseInfo"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ImageResponseInfo
```


Response metadata of an image call. Maps to `response` of `doGenerate` on an image model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L116)

## Constructors

<a id="Vercel_AI_Provider_ImageResponseInfo__ctor_"></a>
<a id="Vercel_AI_Provider_ImageResponseInfo__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `ImageResponseInfo(DateTimeOffset, string?, IReadOnlyDictionary<string, string>?)`


```csharp
public ImageResponseInfo(DateTimeOffset timestamp, string? modelId, IReadOnlyDictionary<string, string>? headers)
```


Creates response info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L119)

## Properties

<a id="Vercel_AI_Provider_ImageResponseInfo_Headers_"></a>
<a id="Vercel_AI_Provider_ImageResponseInfo_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L133)

<a id="Vercel_AI_Provider_ImageResponseInfo_ModelId_"></a>
<a id="Vercel_AI_Provider_ImageResponseInfo_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; }
```


Model id the provider reported.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L130)

<a id="Vercel_AI_Provider_ImageResponseInfo_Timestamp_"></a>
<a id="Vercel_AI_Provider_ImageResponseInfo_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Time the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L127)
