---
title: "ConvertToModelMessagesOptions"
description: "Options of ConvertToModelMessagesOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ConvertToModelMessagesOptions"
  hidden: true
---

<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class ConvertToModelMessagesOptions
```


Options of `ConvertToModelMessagesOptions)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ConvertToModelMessages.cs#L12)

## Properties

<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions_ConvertDataPart_"></a>
<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions_ConvertDataPart"></a>

### ConvertDataPart


```csharp
public Func<DataUIPart, UserContentPart?>? ConvertDataPart { get; set; }
```


Converts a data part to a text or file part. Returns null if the part should be ignored.

**Value:** [Func&lt;DataUIPart, UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ConvertToModelMessages.cs#L21)

<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions_IgnoreIncompleteToolCalls_"></a>
<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions_IgnoreIncompleteToolCalls"></a>

### IgnoreIncompleteToolCalls


```csharp
public bool IgnoreIncompleteToolCalls { get; set; }
```


Whether to ignore incomplete tool calls. Default is `false`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ConvertToModelMessages.cs#L18)

<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions_Tools_"></a>
<a id="Vercel_AI_Ui_ConvertToModelMessagesOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyDictionary<string, Tool>? Tools { get; set; }
```


The tools to use, by name.

**Value:** [IReadOnlyDictionary&lt;string, Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ConvertToModelMessages.cs#L15)
