---
title: "ErrorMessage"
description: "Formats an unknown thrown value the way getErrorMessage does."
editUrl: false
prev: false
next: false
sidebar:
  label: "ErrorMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_ErrorMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public static class ErrorMessage
```


Formats an unknown thrown value the way `getErrorMessage` does.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L11)

## Methods

<a id="Vercel_AI_Provider_ErrorMessage_GetErrorMessage_"></a>
<a id="Vercel_AI_Provider_ErrorMessage_GetErrorMessage_System_Object_"></a>

### GetErrorMessage(object?)


```csharp
public static string GetErrorMessage(object? error)
```


Returns a display string for &lt;code class="paramref"&gt;error&lt;/code&gt;.
Null and undefined become `unknown error`, strings are returned as-is,
exceptions use [ToString()](https://learn.microsoft.com/dotnet/api/system.exception.tostring), and other values are JSON.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L18)
