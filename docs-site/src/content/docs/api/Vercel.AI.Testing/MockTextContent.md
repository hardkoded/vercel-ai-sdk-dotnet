---
title: "MockTextContent"
description: "Text content returned by a mock generate result."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockTextContent"
  hidden: true
---

<a id="Vercel_AI_Testing_MockTextContent"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockTextContent
```


Text content returned by a mock generate result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L13)

## Constructors

<a id="Vercel_AI_Testing_MockTextContent__ctor_"></a>
<a id="Vercel_AI_Testing_MockTextContent__ctor_System_String_"></a>

### MockTextContent(string)


```csharp
public MockTextContent(string text)
```


Creates text content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L16)

## Properties

<a id="Vercel_AI_Testing_MockTextContent_Text_"></a>
<a id="Vercel_AI_Testing_MockTextContent_Text"></a>

### Text


```csharp
public string Text { get; }
```


Generated text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L28)

<a id="Vercel_AI_Testing_MockTextContent_Type_"></a>
<a id="Vercel_AI_Testing_MockTextContent_Type"></a>

### Type


```csharp
public string Type { get; }
```


Always `text`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L22)
