---
title: "UploadedFile"
description: "An uploaded file."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadedFile"
  hidden: true
---

<a id="Vercel_AI_Provider_UploadedFile"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class UploadedFile
```


An uploaded file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L618)

## Constructors

<a id="Vercel_AI_Provider_UploadedFile__ctor_"></a>
<a id="Vercel_AI_Provider_UploadedFile__ctor_System_String_System_String_"></a>

### UploadedFile(string, string?)


```csharp
public UploadedFile(string id, string? fileName)
```


Creates an uploaded file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L621)

## Properties

<a id="Vercel_AI_Provider_UploadedFile_FileName_"></a>
<a id="Vercel_AI_Provider_UploadedFile_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


File name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L631)

<a id="Vercel_AI_Provider_UploadedFile_Id_"></a>
<a id="Vercel_AI_Provider_UploadedFile_Id"></a>

### Id


```csharp
public string Id { get; }
```


Provider file id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L628)
