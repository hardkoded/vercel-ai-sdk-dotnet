---
title: "ToolCallerBinding"
description: "How a tool may be called."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallerBinding"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolCallerBinding"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolCallerBinding
```


How a tool may be called.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L66)

## Constructors

<a id="Vercel_AI_Operations_ToolCallerBinding__ctor_"></a>
<a id="Vercel_AI_Operations_ToolCallerBinding__ctor_System_String_System_Boolean_"></a>

### ToolCallerBinding(string, bool)


```csharp
public ToolCallerBinding(string type, bool hasPrepareModelMessage)
```


Creates a binding.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `hasPrepareModelMessage` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L69)

## Properties

<a id="Vercel_AI_Operations_ToolCallerBinding_Bind_"></a>
<a id="Vercel_AI_Operations_ToolCallerBinding_Bind"></a>

### Bind


```csharp
public Action<IReadOnlyList<string>>? Bind { get; set; }
```


Records the tools bound for one step and returns the bound tool.

**Value:** [Action&lt;IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L85)

<a id="Vercel_AI_Operations_ToolCallerBinding_HasPrepareModelMessage_"></a>
<a id="Vercel_AI_Operations_ToolCallerBinding_HasPrepareModelMessage"></a>

### HasPrepareModelMessage


```csharp
public bool HasPrepareModelMessage { get; }
```


True when code mode can publish a catalog message.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L79)

<a id="Vercel_AI_Operations_ToolCallerBinding_PrepareModelMessage_"></a>
<a id="Vercel_AI_Operations_ToolCallerBinding_PrepareModelMessage"></a>

### PrepareModelMessage


```csharp
public Func<IReadOnlyList<string>, string>? PrepareModelMessage { get; set; }
```


Builds the catalog message from the tools visible to this caller.

**Value:** [Func&lt;IReadOnlyList&lt;string&gt;, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L82)

<a id="Vercel_AI_Operations_ToolCallerBinding_Type_"></a>
<a id="Vercel_AI_Operations_ToolCallerBinding_Type"></a>

### Type


```csharp
public string Type { get; }
```


`local` or `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L76)
