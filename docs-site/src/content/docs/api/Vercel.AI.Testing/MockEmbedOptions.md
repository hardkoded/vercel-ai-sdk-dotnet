---
title: "MockEmbedOptions"
description: "Embedding values passed to a mock embedding model."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockEmbedOptions"
  hidden: true
---

<a id="Vercel_AI_Testing_MockEmbedOptions"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockEmbedOptions
```


Embedding values passed to a mock embedding model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L90)

## Constructors

<a id="Vercel_AI_Testing_MockEmbedOptions__ctor_"></a>
<a id="Vercel_AI_Testing_MockEmbedOptions__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `MockEmbedOptions(IReadOnlyList<string>?)`


```csharp
public MockEmbedOptions(IReadOnlyList<string>? values)
```


Creates embed options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L93)

## Properties

<a id="Vercel_AI_Testing_MockEmbedOptions_Values_"></a>
<a id="Vercel_AI_Testing_MockEmbedOptions_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Input values.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L99)
