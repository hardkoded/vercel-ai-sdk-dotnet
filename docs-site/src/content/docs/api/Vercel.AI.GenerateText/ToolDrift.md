---
title: "ToolDrift"
description: "Added, removed, and changed tool names."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolDrift"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolDrift"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolDrift
```


Added, removed, and changed tool names.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L37)

## Constructors

<a id="Vercel_AI_GenerateText_ToolDrift__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolDrift__ctor_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `ToolDrift(IReadOnlyList<string>, IReadOnlyList<string>, IReadOnlyList<string>)`


```csharp
public ToolDrift(IReadOnlyList<string> added, IReadOnlyList<string> removed, IReadOnlyList<string> changed)
```


Creates a drift report.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `added` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `removed` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `changed` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L40)

## Properties

<a id="Vercel_AI_GenerateText_ToolDrift_Added_"></a>
<a id="Vercel_AI_GenerateText_ToolDrift_Added"></a>

### Added


```csharp
public IReadOnlyList<string> Added { get; }
```


Tools present only in the current map.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L48)

<a id="Vercel_AI_GenerateText_ToolDrift_Changed_"></a>
<a id="Vercel_AI_GenerateText_ToolDrift_Changed"></a>

### Changed


```csharp
public IReadOnlyList<string> Changed { get; }
```


Tools whose digest changed.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L54)

<a id="Vercel_AI_GenerateText_ToolDrift_Removed_"></a>
<a id="Vercel_AI_GenerateText_ToolDrift_Removed"></a>

### Removed


```csharp
public IReadOnlyList<string> Removed { get; }
```


Tools present only in the baseline.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L51)
