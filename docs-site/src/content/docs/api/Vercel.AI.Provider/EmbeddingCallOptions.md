---
title: "EmbeddingCallOptions"
description: "Per-call embedding settings. Maps to the options of doEmbed."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingCallOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_EmbeddingCallOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class EmbeddingCallOptions
```


Per-call embedding settings. Maps to the options of `doEmbed`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L54)

## Constructors

<a id="Vercel_AI_Provider_EmbeddingCallOptions__ctor_"></a>
<a id="Vercel_AI_Provider_EmbeddingCallOptions__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `EmbeddingCallOptions(IReadOnlyList<string>)`


```csharp
public EmbeddingCallOptions(IReadOnlyList<string> values)
```


Creates call options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L57)

## Properties

<a id="Vercel_AI_Provider_EmbeddingCallOptions_Dimensions_"></a>
<a id="Vercel_AI_Provider_EmbeddingCallOptions_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; set; }
```


Requested vector width. A provider option for the same setting wins.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L72)

<a id="Vercel_AI_Provider_EmbeddingCallOptions_Headers_"></a>
<a id="Vercel_AI_Provider_EmbeddingCallOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L69)

<a id="Vercel_AI_Provider_EmbeddingCallOptions_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_EmbeddingCallOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options, keyed by provider id.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L66)

<a id="Vercel_AI_Provider_EmbeddingCallOptions_User_"></a>
<a id="Vercel_AI_Provider_EmbeddingCallOptions_User"></a>

### User


```csharp
public string? User { get; set; }
```


End-user identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L75)

<a id="Vercel_AI_Provider_EmbeddingCallOptions_Values_"></a>
<a id="Vercel_AI_Provider_EmbeddingCallOptions_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Values to embed.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L63)
