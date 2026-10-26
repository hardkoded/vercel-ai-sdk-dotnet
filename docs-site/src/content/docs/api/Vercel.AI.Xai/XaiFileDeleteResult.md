---
title: "XaiFileDeleteResult"
description: "Result of deleting an xAI file."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFileDeleteResult"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFileDeleteResult"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFileDeleteResult
```


Result of deleting an xAI file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L148)

## Constructors

<a id="Vercel_AI_Xai_XaiFileDeleteResult__ctor_"></a>
<a id="Vercel_AI_Xai_XaiFileDeleteResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Boolean_"></a>

### `XaiFileDeleteResult(IReadOnlyDictionary<string, string>, bool)`


```csharp
public XaiFileDeleteResult(IReadOnlyDictionary<string, string> providerReference, bool deleted)
```


Creates a delete result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `deleted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L151)

## Properties

<a id="Vercel_AI_Xai_XaiFileDeleteResult_Deleted_"></a>
<a id="Vercel_AI_Xai_XaiFileDeleteResult_Deleted"></a>

### Deleted


```csharp
public bool Deleted { get; }
```


Whether the provider deleted the file.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L165)

<a id="Vercel_AI_Xai_XaiFileDeleteResult_ProviderReference_"></a>
<a id="Vercel_AI_Xai_XaiFileDeleteResult_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Reference containing the deleted `xai` file id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L162)

<a id="Vercel_AI_Xai_XaiFileDeleteResult_Warnings_"></a>
<a id="Vercel_AI_Xai_XaiFileDeleteResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Always empty.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L159)
