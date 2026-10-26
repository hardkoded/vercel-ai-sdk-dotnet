---
title: "GoogleSystemTurn"
description: "A system turn."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSystemTurn"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSystemTurn"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSystemTurn : GoogleTurn
```


A system turn.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleTurn](/api/Vercel.AI.Google/GoogleTurn/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L61)

## Constructors

<a id="Vercel_AI_Google_GoogleSystemTurn__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSystemTurn__ctor_System_String_"></a>

### GoogleSystemTurn(string)


```csharp
public GoogleSystemTurn(string text)
```


Creates a system turn.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L64)

## Properties

<a id="Vercel_AI_Google_GoogleSystemTurn_Text_"></a>
<a id="Vercel_AI_Google_GoogleSystemTurn_Text"></a>

### Text


```csharp
public string Text { get; }
```


Instruction text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L70)
