---
title: "GoogleToolTurn"
description: "A tool-result turn."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolTurn"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolTurn"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolTurn : GoogleTurn
```


A tool-result turn.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleTurn](/api/Vercel.AI.Google/GoogleTurn/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L236)

## Constructors

<a id="Vercel_AI_Google_GoogleToolTurn__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolTurn__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleToolResultPart__"></a>

### `GoogleToolTurn(IReadOnlyList<GoogleToolResultPart>)`


```csharp
public GoogleToolTurn(IReadOnlyList<GoogleToolResultPart> results)
```


Creates a tool turn.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `results` | [IReadOnlyList&lt;GoogleToolResultPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L239)

## Properties

<a id="Vercel_AI_Google_GoogleToolTurn_Results_"></a>
<a id="Vercel_AI_Google_GoogleToolTurn_Results"></a>

### Results


```csharp
public IReadOnlyList<GoogleToolResultPart> Results { get; }
```


Tool results.

**Value:** [IReadOnlyList&lt;GoogleToolResultPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L245)
