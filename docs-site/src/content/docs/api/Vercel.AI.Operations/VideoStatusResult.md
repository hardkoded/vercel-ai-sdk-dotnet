---
title: "VideoStatusResult"
description: "Result of doStatus."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoStatusResult"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoStatusResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoStatusResult
```


Result of `doStatus`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L308)

## Constructors

<a id="Vercel_AI_Operations_VideoStatusResult__ctor_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoOutput__System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `VideoStatusResult(string, IReadOnlyList<VideoOutput>?, string?, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?)`


```csharp
public VideoStatusResult(string status, IReadOnlyList<VideoOutput>? videos = null, string? error = null, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a status result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `videos` | [IReadOnlyList&lt;VideoOutput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `error` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L311)

## Properties

<a id="Vercel_AI_Operations_VideoStatusResult_Error_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult_Error"></a>

### Error


```csharp
public string? Error { get; }
```


Error text when [Status](#Vercel_AI_Operations_VideoStatusResult_Status) is `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L328)

<a id="Vercel_AI_Operations_VideoStatusResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L334)

<a id="Vercel_AI_Operations_VideoStatusResult_Response_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L337)

<a id="Vercel_AI_Operations_VideoStatusResult_Status_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult_Status"></a>

### Status


```csharp
public string Status { get; }
```


`pending`, `completed`, or `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L322)

<a id="Vercel_AI_Operations_VideoStatusResult_Videos_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult_Videos"></a>

### Videos


```csharp
public IReadOnlyList<VideoOutput> Videos { get; }
```


Videos when completed.

**Value:** [IReadOnlyList&lt;VideoOutput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L325)

<a id="Vercel_AI_Operations_VideoStatusResult_Warnings_"></a>
<a id="Vercel_AI_Operations_VideoStatusResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L331)
