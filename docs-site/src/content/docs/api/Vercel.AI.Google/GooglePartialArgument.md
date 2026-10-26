---
title: "GooglePartialArgument"
description: "One streamed function-call argument fragment."
editUrl: false
prev: false
next: false
sidebar:
  label: "GooglePartialArgument"
  hidden: true
---

<a id="Vercel_AI_Google_GooglePartialArgument"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GooglePartialArgument
```


One streamed function-call argument fragment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L13)

## Properties

<a id="Vercel_AI_Google_GooglePartialArgument_BoolValue_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_BoolValue"></a>

### BoolValue


```csharp
public bool? BoolValue { get; set; }
```


Boolean fragment.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L28)

<a id="Vercel_AI_Google_GooglePartialArgument_HasNull_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_HasNull"></a>

### HasNull


```csharp
public bool HasNull { get; set; }
```


True when the fragment is JSON null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L31)

<a id="Vercel_AI_Google_GooglePartialArgument_HasString_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_HasString"></a>

### HasString


```csharp
public bool HasString { get; set; }
```


True when [StringValue](#Vercel_AI_Google_GooglePartialArgument_StringValue) was supplied, including an empty string.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L22)

<a id="Vercel_AI_Google_GooglePartialArgument_JsonPath_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_JsonPath"></a>

### JsonPath


```csharp
public string JsonPath { get; set; }
```


JSONPath such as `$.recipe.ingredients[0].name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L16)

<a id="Vercel_AI_Google_GooglePartialArgument_NumberValue_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_NumberValue"></a>

### NumberValue


```csharp
public double? NumberValue { get; set; }
```


Number fragment.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L25)

<a id="Vercel_AI_Google_GooglePartialArgument_StringValue_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_StringValue"></a>

### StringValue


```csharp
public string? StringValue { get; set; }
```


String fragment, when this chunk carries one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L19)

<a id="Vercel_AI_Google_GooglePartialArgument_WillContinue_"></a>
<a id="Vercel_AI_Google_GooglePartialArgument_WillContinue"></a>

### WillContinue


```csharp
public bool? WillContinue { get; set; }
```


True when a string value will be continued by a later chunk.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L34)
