---
title: "ApprovalHistoryPart"
description: "One part of an approval history message."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApprovalHistoryPart"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ApprovalHistoryPart"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public abstract class ApprovalHistoryPart
```


One part of an approval history message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ApprovalRequestPart](/api/Vercel.AI.GenerateText/ApprovalRequestPart/), [ApprovalResponsePart](/api/Vercel.AI.GenerateText/ApprovalResponsePart/), [ApprovalToolCallPart](/api/Vercel.AI.GenerateText/ApprovalToolCallPart/), [ApprovalToolResultPart](/api/Vercel.AI.GenerateText/ApprovalToolResultPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L11)

## Constructors

<a id="Vercel_AI_GenerateText_ApprovalHistoryPart__ctor_"></a>
<a id="Vercel_AI_GenerateText_ApprovalHistoryPart__ctor_System_String_"></a>

### ApprovalHistoryPart(string)


```csharp
protected ApprovalHistoryPart(string type)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L14)

## Properties

<a id="Vercel_AI_GenerateText_ApprovalHistoryPart_Type_"></a>
<a id="Vercel_AI_GenerateText_ApprovalHistoryPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L20)
