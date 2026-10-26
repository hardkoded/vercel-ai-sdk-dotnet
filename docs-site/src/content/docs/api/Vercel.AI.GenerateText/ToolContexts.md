---
title: "ToolContexts"
description: "Validates tool context. Maps to validateToolContext."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolContexts"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolContexts"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolContexts
```


Validates tool context. Maps to `validateToolContext`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L45)

## Methods

<a id="Vercel_AI_GenerateText_ToolContexts_ValidateToolContext_"></a>
<a id="Vercel_AI_GenerateText_ToolContexts_ValidateToolContext__1_System_String___0_System_Func___0_System_Boolean__"></a>

### `ValidateToolContext<T>(string, T, Func<T, bool>?)`


```csharp
public static T ValidateToolContext<T>(string toolName, T context, Func<T, bool>? contextSchema)
```


Returns &lt;code class="paramref"&gt;context&lt;/code&gt; when &lt;code class="paramref"&gt;contextSchema&lt;/code&gt; is null.
Otherwise returns it when the schema accepts it, and throws [TypeValidationException](/api/Vercel.AI.GenerateText/TypeValidationException/) when it does not.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `context` | `T` |  |
| `contextSchema` | [Func&lt;T, bool&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L51)
