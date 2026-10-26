---
title: "UploadFileCall"
description: "Arguments forwarded to a files API."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadFileCall"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadFileCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadFileCall
```


Arguments forwarded to a files API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L61)

## Constructors

<a id="Vercel_AI_Operations_UploadFileCall__ctor_"></a>
<a id="Vercel_AI_Operations_UploadFileCall__ctor_Vercel_AI_Operations_UploadData_System_String_System_String_System_Threading_CancellationToken_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__"></a>

### `UploadFileCall(UploadData, string, string?, CancellationToken, IReadOnlyDictionary<string, string>?, JsonElement?)`


```csharp
public UploadFileCall(UploadData data, string mediaType, string? filename, CancellationToken cancellationToken, IReadOnlyDictionary<string, string>? headers, JsonElement? providerOptions)
```


Creates an upload call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [UploadData](/api/Vercel.AI.Operations/UploadData/) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `filename` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L64)

## Properties

<a id="Vercel_AI_Operations_UploadFileCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_UploadFileCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L84)

<a id="Vercel_AI_Operations_UploadFileCall_Data_"></a>
<a id="Vercel_AI_Operations_UploadFileCall_Data"></a>

### Data


```csharp
public UploadData Data { get; }
```


Tagged data.

**Value:** [UploadData](/api/Vercel.AI.Operations/UploadData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L75)

<a id="Vercel_AI_Operations_UploadFileCall_Filename_"></a>
<a id="Vercel_AI_Operations_UploadFileCall_Filename"></a>

### Filename


```csharp
public string? Filename { get; }
```


Optional filename.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L81)

<a id="Vercel_AI_Operations_UploadFileCall_Headers_"></a>
<a id="Vercel_AI_Operations_UploadFileCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Request headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L87)

<a id="Vercel_AI_Operations_UploadFileCall_MediaType_"></a>
<a id="Vercel_AI_Operations_UploadFileCall_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type sent to the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L78)

<a id="Vercel_AI_Operations_UploadFileCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_UploadFileCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options. Null when the caller omitted them.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L90)
