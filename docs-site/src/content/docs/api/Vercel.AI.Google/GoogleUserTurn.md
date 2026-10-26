---
title: "GoogleUserTurn"
description: "A user turn."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleUserTurn"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleUserTurn"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleUserTurn : GoogleTurn
```


A user turn.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleTurn](/api/Vercel.AI.Google/GoogleTurn/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L74)

## Constructors

<a id="Vercel_AI_Google_GoogleUserTurn__ctor_"></a>
<a id="Vercel_AI_Google_GoogleUserTurn__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleUserPart__"></a>

### `GoogleUserTurn(IReadOnlyList<GoogleUserPart>)`


```csharp
public GoogleUserTurn(IReadOnlyList<GoogleUserPart> parts)
```


Creates a user turn.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parts` | [IReadOnlyList&lt;GoogleUserPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L77)

## Properties

<a id="Vercel_AI_Google_GoogleUserTurn_Parts_"></a>
<a id="Vercel_AI_Google_GoogleUserTurn_Parts"></a>

### Parts


```csharp
public IReadOnlyList<GoogleUserPart> Parts { get; }
```


User parts.

**Value:** [IReadOnlyList&lt;GoogleUserPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L83)
