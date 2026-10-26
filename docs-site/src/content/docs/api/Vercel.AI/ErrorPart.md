---
title: "ErrorPart"
description: "The stream failed."
editUrl: false
prev: false
next: false
sidebar:
  label: "ErrorPart"
  hidden: true
---

<a id="Vercel_AI_ErrorPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ErrorPart : TextStreamPart
```


The stream failed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1425)

## Constructors

<a id="Vercel_AI_ErrorPart__ctor_"></a>
<a id="Vercel_AI_ErrorPart__ctor_System_String_"></a>

### ErrorPart(string)


```csharp
public ErrorPart(string message)
```


Creates an error part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1428)

## Properties

<a id="Vercel_AI_ErrorPart_Message_"></a>
<a id="Vercel_AI_ErrorPart_Message"></a>

### Message


```csharp
public string Message { get; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1435)
