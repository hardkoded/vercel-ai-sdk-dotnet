---
title: "HostnameParts"
description: "Checks a value before it is concatenated into a generated host."
editUrl: false
prev: false
next: false
sidebar:
  label: "HostnameParts"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_HostnameParts"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class HostnameParts
```


Checks a value before it is concatenated into a generated host.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/HostnameParts.cs#L10)

## Methods

<a id="Vercel_AI_ProviderUtils_HostnameParts_IsValidHostnamePart_"></a>
<a id="Vercel_AI_ProviderUtils_HostnameParts_IsValidHostnamePart_System_String_"></a>

### IsValidHostnamePart(string?)


```csharp
public static bool IsValidHostnamePart(string? value)
```


Returns whether &lt;code class="paramref"&gt;value&lt;/code&gt; is one DNS label: 1–63 letters, digits, or hyphens, with no leading or trailing hyphen.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/HostnameParts.cs#L18)
