---
title: "UIMessageChunkException"
description: "Thrown when a UI message chunk does not match the protocol schema."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageChunkException"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageChunkException"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageChunkException : Exception, ISerializable
```


Thrown when a UI message chunk does not match the protocol schema.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunks.cs#L11)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageChunkException__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageChunkException__ctor_System_String_"></a>

### UIMessageChunkException(string)


```csharp
public UIMessageChunkException(string message)
```


Creates an exception with &lt;code class="paramref"&gt;message&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageChunks.cs#L14)
