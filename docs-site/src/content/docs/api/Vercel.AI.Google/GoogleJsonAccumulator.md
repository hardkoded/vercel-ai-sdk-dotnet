---
title: "GoogleJsonAccumulator"
description: "Builds a JSON object from Gemini streaming partialArgs chunks. Concatenated text deltas plus Finalize() match the serialized object. Keys such as proto are s..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleJsonAccumulator"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleJsonAccumulator"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleJsonAccumulator
```


Builds a JSON object from Gemini streaming `partialArgs` chunks.
Concatenated text deltas plus [Finalize()](#Vercel_AI_Google_GoogleJsonAccumulator_Finalize) match the serialized object.
Keys such as `__proto__` are stored as ordinary properties.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L76)

## Methods

<a id="Vercel_AI_Google_GoogleJsonAccumulator_Finalize_"></a>
<a id="Vercel_AI_Google_GoogleJsonAccumulator_Finalize"></a>

### Finalize()


```csharp
public GooglePartialArgumentsFinal Finalize()
```


Closes any open containers and returns the suffix required to finish the JSON text.

**Returns:** [GooglePartialArgumentsFinal](/api/Vercel.AI.Google/GooglePartialArgumentsFinal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L101)

<a id="Vercel_AI_Google_GoogleJsonAccumulator_Process_"></a>
<a id="Vercel_AI_Google_GoogleJsonAccumulator_Process_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GooglePartialArgument__"></a>

### `Process(IReadOnlyList<GooglePartialArgument>?)`


```csharp
public GooglePartialArgumentsUpdate Process(IReadOnlyList<GooglePartialArgument>? partialArgs)
```


Applies one batch of partial arguments.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `partialArgs` | [IReadOnlyList&lt;GooglePartialArgument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [GooglePartialArgumentsUpdate](/api/Vercel.AI.Google/GooglePartialArgumentsUpdate/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonAccumulator.cs#L84)
