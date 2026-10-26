---
title: "ProviderReferences"
description: "Helpers for provider references: maps from provider name to a provider-specific id."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderReferences"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderReferences"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ProviderReferences
```


Helpers for provider references: maps from provider name to a provider-specific id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L11)

## Methods

<a id="Vercel_AI_ProviderUtils_ProviderReferences_IsProviderReference_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderReferences_IsProviderReference_System_Object_"></a>

### IsProviderReference(object?)


```csharp
public static bool IsProviderReference(object? data)
```


True for a plain dictionary without a `type` key. Strings, URLs, bytes, arrays, and primitives are not references.
Maps to `isProviderReference`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L17)

<a id="Vercel_AI_ProviderUtils_ProviderReferences_ResolveProviderReference_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderReferences_ResolveProviderReference_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `ResolveProviderReference(IReadOnlyDictionary<string, string>, string)`


```csharp
public static string ResolveProviderReference(IReadOnlyDictionary<string, string> reference, string provider)
```


Returns the id for &lt;code class="paramref"&gt;provider&lt;/code&gt;. Maps to `resolveProviderReference`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

**Exceptions**

| Type | Condition |
| --- | --- |
| [NoSuchProviderReferenceError](/api/Vercel.AI.ProviderUtils/NoSuchProviderReferenceError/) | The reference has no entry for &lt;code class="paramref"&gt;provider&lt;/code&gt;. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderReferences.cs#L24)
