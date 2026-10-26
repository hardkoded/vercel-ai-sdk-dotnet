---
title: "GooglePartialArgumentsFinal"
description: "The completed argument JSON and the suffix still needed to close it."
editUrl: false
prev: false
next: false
sidebar:
  label: "GooglePartialArgumentsFinal"
  hidden: true
---

<a id="Vercel_AI_Google_GooglePartialArgumentsFinal"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GooglePartialArgumentsFinal
```


The completed argument JSON and the suffix still needed to close it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L55)

## Constructors

<a id="Vercel_AI_Google_GooglePartialArgumentsFinal__ctor_"></a>
<a id="Vercel_AI_Google_GooglePartialArgumentsFinal__ctor_System_String_System_String_"></a>

### GooglePartialArgumentsFinal(string, string)


```csharp
public GooglePartialArgumentsFinal(string finalJson, string closingDelta)
```


Creates a finalization result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finalJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `closingDelta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L58)

## Properties

<a id="Vercel_AI_Google_GooglePartialArgumentsFinal_ClosingDelta_"></a>
<a id="Vercel_AI_Google_GooglePartialArgumentsFinal_ClosingDelta"></a>

### ClosingDelta


```csharp
public string ClosingDelta { get; }
```


Characters that close the streamed text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L68)

<a id="Vercel_AI_Google_GooglePartialArgumentsFinal_FinalJson_"></a>
<a id="Vercel_AI_Google_GooglePartialArgumentsFinal_FinalJson"></a>

### FinalJson


```csharp
public string FinalJson { get; }
```


Complete JSON object.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L65)
