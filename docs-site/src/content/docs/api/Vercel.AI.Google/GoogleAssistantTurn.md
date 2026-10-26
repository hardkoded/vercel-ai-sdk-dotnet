---
title: "GoogleAssistantTurn"
description: "An assistant turn."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleAssistantTurn"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleAssistantTurn"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleAssistantTurn : GoogleTurn
```


An assistant turn.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleTurn](/api/Vercel.AI.Google/GoogleTurn/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L137)

## Constructors

<a id="Vercel_AI_Google_GoogleAssistantTurn__ctor_"></a>
<a id="Vercel_AI_Google_GoogleAssistantTurn__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleAssistantPart__"></a>

### `GoogleAssistantTurn(IReadOnlyList<GoogleAssistantPart>)`


```csharp
public GoogleAssistantTurn(IReadOnlyList<GoogleAssistantPart> parts)
```


Creates an assistant turn.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parts` | [IReadOnlyList&lt;GoogleAssistantPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L140)

## Properties

<a id="Vercel_AI_Google_GoogleAssistantTurn_Parts_"></a>
<a id="Vercel_AI_Google_GoogleAssistantTurn_Parts"></a>

### Parts


```csharp
public IReadOnlyList<GoogleAssistantPart> Parts { get; }
```


Assistant parts.

**Value:** [IReadOnlyList&lt;GoogleAssistantPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L146)
