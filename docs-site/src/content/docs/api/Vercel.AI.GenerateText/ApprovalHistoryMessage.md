---
title: "ApprovalHistoryMessage"
description: "A message in the approval history."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApprovalHistoryMessage"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ApprovalHistoryMessage
```


A message in the approval history.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L116)

## Constructors

<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage__ctor_"></a>
<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_ApprovalHistoryPart__"></a>

### `ApprovalHistoryMessage(string, string?, IReadOnlyList<ApprovalHistoryPart>?)`


```csharp
public ApprovalHistoryMessage(string role, string? text = null, IReadOnlyList<ApprovalHistoryPart>? parts = null)
```


Creates a message. &lt;code class="paramref"&gt;parts&lt;/code&gt; is null when the message content is a string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `role` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `parts` | [IReadOnlyList&lt;ApprovalHistoryPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L119)

## Properties

<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage_Parts_"></a>
<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage_Parts"></a>

### Parts


```csharp
public IReadOnlyList<ApprovalHistoryPart>? Parts { get; }
```


Structured content. Null means the message is not a part list.

**Value:** [IReadOnlyList&lt;ApprovalHistoryPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L133)

<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage_Role_"></a>
<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage_Role"></a>

### Role


```csharp
public string Role { get; }
```


Message role.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L127)

<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage_Text_"></a>
<a id="Vercel_AI_GenerateText_ApprovalHistoryMessage_Text"></a>

### Text


```csharp
public string? Text { get; }
```


String content.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L130)
