---
title: "DecisionBox"
description: "Edges in [0, 1], measured from the image's top-left corner."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionBox"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionBox"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionBox
```


Edges in [0, 1], measured from the image's top-left corner.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L84)

## Constructors

<a id="Vercel_AI_Operations_DecisionBox__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionBox__ctor_System_Double_System_Double_System_Double_System_Double_"></a>

### DecisionBox(double, double, double, double)


```csharp
public DecisionBox(double top, double bottom, double left, double right)
```


Creates a box.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `top` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `bottom` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `left` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `right` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L87)

## Properties

<a id="Vercel_AI_Operations_DecisionBox_Bottom_"></a>
<a id="Vercel_AI_Operations_DecisionBox_Bottom"></a>

### Bottom


```csharp
public double Bottom { get; }
```


Bottom edge.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L99)

<a id="Vercel_AI_Operations_DecisionBox_Left_"></a>
<a id="Vercel_AI_Operations_DecisionBox_Left"></a>

### Left


```csharp
public double Left { get; }
```


Left edge.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L102)

<a id="Vercel_AI_Operations_DecisionBox_Right_"></a>
<a id="Vercel_AI_Operations_DecisionBox_Right"></a>

### Right


```csharp
public double Right { get; }
```


Right edge.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L105)

<a id="Vercel_AI_Operations_DecisionBox_Top_"></a>
<a id="Vercel_AI_Operations_DecisionBox_Top"></a>

### Top


```csharp
public double Top { get; }
```


Top edge.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L96)
