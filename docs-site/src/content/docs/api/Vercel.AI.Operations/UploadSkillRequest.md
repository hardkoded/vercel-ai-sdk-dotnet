---
title: "UploadSkillRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadSkillRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadSkillRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadSkillRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L434)

## Properties

<a id="Vercel_AI_Operations_UploadSkillRequest_Api_"></a>
<a id="Vercel_AI_Operations_UploadSkillRequest_Api"></a>

### Api


```csharp
public ApiOrProvider<ISkillStore, ISkillStoreProvider>? Api { get; set; }
```


Skills API or a provider that exposes one.

**Value:** `ApiOrProvider&lt;ISkillStore, ISkillStoreProvider&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L437)

<a id="Vercel_AI_Operations_UploadSkillRequest_DisplayTitle_"></a>
<a id="Vercel_AI_Operations_UploadSkillRequest_DisplayTitle"></a>

### DisplayTitle


```csharp
public string? DisplayTitle { get; set; }
```


Display title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L443)

<a id="Vercel_AI_Operations_UploadSkillRequest_Files_"></a>
<a id="Vercel_AI_Operations_UploadSkillRequest_Files"></a>

### Files


```csharp
public IReadOnlyList<SkillFile> Files { get; set; }
```


Skill files.

**Value:** [IReadOnlyList&lt;SkillFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L440)

<a id="Vercel_AI_Operations_UploadSkillRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_UploadSkillRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L446)
