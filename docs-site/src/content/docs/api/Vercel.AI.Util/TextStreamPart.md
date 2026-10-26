---
title: "TextStreamPart"
description: "One part of a model text stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextStreamPart"
  hidden: true
---

<a id="Vercel_AI_Util_TextStreamPart"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class TextStreamPart
```


One part of a model text stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L10)

## Constructors

<a id="Vercel_AI_Util_TextStreamPart__ctor_"></a>
<a id="Vercel_AI_Util_TextStreamPart__ctor_System_String_System_String_System_String_System_String_"></a>

### TextStreamPart(string, string?, string?, string?)


```csharp
public TextStreamPart(string type, string? id = null, string? text = null, string? delta = null)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `delta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L13)

## Properties

<a id="Vercel_AI_Util_TextStreamPart_Delta_"></a>
<a id="Vercel_AI_Util_TextStreamPart_Delta"></a>

### Delta


```csharp
public string? Delta { get; }
```


Delta carried by provider stream parts that use `delta` instead of `text`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L31)

<a id="Vercel_AI_Util_TextStreamPart_Id_"></a>
<a id="Vercel_AI_Util_TextStreamPart_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Part id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L25)

<a id="Vercel_AI_Util_TextStreamPart_Text_"></a>
<a id="Vercel_AI_Util_TextStreamPart_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Text carried by a `text-delta` part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L28)

<a id="Vercel_AI_Util_TextStreamPart_Type_"></a>
<a id="Vercel_AI_Util_TextStreamPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part kind, such as `text-delta`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L22)
