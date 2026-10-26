---
title: "UploadSkillCall"
description: "Arguments for a skill upload."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadSkillCall"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadSkillCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadSkillCall
```


Arguments for a skill upload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L382)

## Constructors

<a id="Vercel_AI_Operations_UploadSkillCall__ctor_"></a>
<a id="Vercel_AI_Operations_UploadSkillCall__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_SkillFilePayload__System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### `UploadSkillCall(IReadOnlyList<SkillFilePayload>, string?, JsonElement?)`


```csharp
public UploadSkillCall(IReadOnlyList<SkillFilePayload> files, string? displayTitle, JsonElement? providerOptions)
```


Creates a skill call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `files` | [IReadOnlyList&lt;SkillFilePayload&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `displayTitle` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L385)

## Properties

<a id="Vercel_AI_Operations_UploadSkillCall_DisplayTitle_"></a>
<a id="Vercel_AI_Operations_UploadSkillCall_DisplayTitle"></a>

### DisplayTitle


```csharp
public string? DisplayTitle { get; }
```


Display title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L396)

<a id="Vercel_AI_Operations_UploadSkillCall_Files_"></a>
<a id="Vercel_AI_Operations_UploadSkillCall_Files"></a>

### Files


```csharp
public IReadOnlyList<SkillFilePayload> Files { get; }
```


Normalized files.

**Value:** [IReadOnlyList&lt;SkillFilePayload&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L393)

<a id="Vercel_AI_Operations_UploadSkillCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_UploadSkillCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L399)
