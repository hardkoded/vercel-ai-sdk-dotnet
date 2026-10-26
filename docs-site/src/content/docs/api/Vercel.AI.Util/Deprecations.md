---
title: "Deprecations"
description: "Stable codes for deprecation warnings."
editUrl: false
prev: false
next: false
sidebar:
  label: "Deprecations"
  hidden: true
---

<a id="Vercel_AI_Util_Deprecations"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class Deprecations
```


Stable codes for deprecation warnings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Deprecations.cs#L11)

## Methods

<a id="Vercel_AI_Util_Deprecations_GetDeprecationCode_"></a>
<a id="Vercel_AI_Util_Deprecations_GetDeprecationCode_System_String_System_String_"></a>

### GetDeprecationCode(string, string?)


```csharp
public static string GetDeprecationCode(string setting, string? provider)
```


Returns the stable code for a deprecated &lt;code class="paramref"&gt;setting&lt;/code&gt;. A &lt;code class="paramref"&gt;provider&lt;/code&gt;
scopes the code to that provider, but not to a model or message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `setting` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Deprecations.cs#L36)
