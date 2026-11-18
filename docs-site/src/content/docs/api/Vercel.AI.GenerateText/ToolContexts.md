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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L46)

## Methods

<a id="Vercel_AI_GenerateText_ToolContexts_ValidateToolContext_"></a>
<a id="Vercel_AI_GenerateText_ToolContexts_ValidateToolContext_System_String_System_Nullable_System_Text_Json_JsonElement__System_Func_System_Text_Json_JsonElement_System_Text_Json_JsonElement__"></a>

### `ValidateToolContext(string, JsonElement?, Func<JsonElement, JsonElement>?)`


```csharp
public static JsonElement? ValidateToolContext(string toolName, JsonElement? context, Func<JsonElement, JsonElement>? contextSchema)
```


Validates a tool context with the context schema of a tool. Returns &lt;code class="paramref"&gt;context&lt;/code&gt; as it is when
&lt;code class="paramref"&gt;contextSchema&lt;/code&gt; is null. Otherwise returns the context that the schema produces, with its defaults applied.
A missing context is validated as a JSON `null`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `context` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `contextSchema` | [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

**Exceptions**

| Type | Condition |
| --- | --- |
| [TypeValidationException](/api/Vercel.AI.GenerateText/TypeValidationException/) | The schema rejected the context. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L58)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L83)
