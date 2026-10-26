---
title: "VideoCallOptions"
description: "Video call settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoCallOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_VideoCallOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class VideoCallOptions
```


Video call settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L410)

## Constructors

<a id="Vercel_AI_Provider_VideoCallOptions__ctor_"></a>
<a id="Vercel_AI_Provider_VideoCallOptions__ctor_System_String_"></a>

### VideoCallOptions(string)


```csharp
public VideoCallOptions(string prompt)
```


Creates video options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L413)

## Properties

<a id="Vercel_AI_Provider_VideoCallOptions_DurationSeconds_"></a>
<a id="Vercel_AI_Provider_VideoCallOptions_DurationSeconds"></a>

### DurationSeconds


```csharp
public int? DurationSeconds { get; set; }
```


Duration hint in seconds.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L422)

<a id="Vercel_AI_Provider_VideoCallOptions_Prompt_"></a>
<a id="Vercel_AI_Provider_VideoCallOptions_Prompt"></a>

### Prompt


```csharp
public string Prompt { get; }
```


Video prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L419)
