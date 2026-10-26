---
title: "ToolExecution.ToolStep<T>"
description: "One tool output."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecution.ToolStep<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ToolExecution.ToolStep<T>
```


One tool output.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L536)

## Constructors

<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1__ctor_System_String__0_"></a>

### ToolStep(string, T)


```csharp
public ToolStep(string type, T output)
```


Creates a step.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `output` | `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L539)

## Properties

<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1_Output_"></a>
<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1_Output"></a>

### Output


```csharp
public T Output { get; }
```


Output value.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L549)

<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1_Type_"></a>
<a id="Vercel_AI_ProviderUtils_ToolExecution_ToolStep_1_Type"></a>

### Type


```csharp
public string Type { get; }
```


`preliminary` or `final`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L546)
