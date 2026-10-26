---
title: "MockStreamChunk"
description: "One chunk of a mock language-model stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockStreamChunk"
  hidden: true
---

<a id="Vercel_AI_Testing_MockStreamChunk"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockStreamChunk
```


One chunk of a mock language-model stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L45)

## Constructors

<a id="Vercel_AI_Testing_MockStreamChunk__ctor_"></a>
<a id="Vercel_AI_Testing_MockStreamChunk__ctor_System_String_System_String_System_String_"></a>

### MockStreamChunk(string, string?, string?)


```csharp
public MockStreamChunk(string type, string? id = null, string? delta = null)
```


Creates a chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `delta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L48)

## Properties

<a id="Vercel_AI_Testing_MockStreamChunk_Delta_"></a>
<a id="Vercel_AI_Testing_MockStreamChunk_Delta"></a>

### Delta


```csharp
public string? Delta { get; }
```


Text delta, when [Type](#Vercel_AI_Testing_MockStreamChunk_Type) is `text-delta`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L62)

<a id="Vercel_AI_Testing_MockStreamChunk_Id_"></a>
<a id="Vercel_AI_Testing_MockStreamChunk_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Chunk id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L59)

<a id="Vercel_AI_Testing_MockStreamChunk_Type_"></a>
<a id="Vercel_AI_Testing_MockStreamChunk_Type"></a>

### Type


```csharp
public string Type { get; }
```


Chunk kind.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L56)
