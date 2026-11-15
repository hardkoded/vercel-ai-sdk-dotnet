---
title: "ProviderOptionsResolver"
description: "Resolves options that a model accepts under more than one providerOptions namespace."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderOptionsResolver"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderOptionsResolver"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ProviderOptionsResolver
```


Resolves options that a model accepts under more than one `providerOptions` namespace.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L34)

## Methods

<a id="Vercel_AI_ProviderUtils_ProviderOptionsResolver_Resolve_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderOptionsResolver_Resolve__1_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Collections_Generic_IEnumerable_System_String__Vercel_AI_ProviderUtils_ProviderOptionsResolution_System_Func_System_Text_Json_JsonElement___0__System_Func___0___0___0__"></a>

### `Resolve<T>(IReadOnlyDictionary<string, JsonElement>?, IEnumerable<string>, ProviderOptionsResolution, Func<JsonElement, T>, Func<T, T, T>?)`


```csharp
public static ResolvedProviderOptions<T> Resolve<T>(IReadOnlyDictionary<string, JsonElement>? providerOptions, IEnumerable<string> namespaces, ProviderOptionsResolution resolution, Func<JsonElement, T> parse, Func<T, T, T>? merge = null)
```


Parses provider options that a model accepts under more than one `providerOptions` namespace, for
example a canonical key plus a provider-specific or legacy alias. Maps to `resolveProviderOptions`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Provider options of the call. |
| `namespaces` | [IEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) | Namespaces to look at, in resolution order. Duplicates are ignored. |
| `resolution` | [ProviderOptionsResolution](/api/Vercel.AI.ProviderUtils/ProviderOptionsResolution/) | How present namespaces are combined. |
| `parse` | [Func&lt;JsonElement, T&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Validates and parses the options of one namespace. |
| `merge` | [Func&lt;T, T, T&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) | Combines two parsed options, the second one winning. Required for [Merge](/api/Vercel.AI.ProviderUtils/ProviderOptionsResolution/#Vercel_AI_ProviderUtils_ProviderOptionsResolution_Merge). |

**Returns:** `ResolvedProviderOptions&lt;T&gt;` — The resolved options and the namespaces that contributed to them.

Each present namespace is validated by &lt;code class="paramref"&gt;parse&lt;/code&gt;, which stands in for the upstream schema.
A namespace that is missing or `null` is skipped, and an invalid one makes &lt;code class="paramref"&gt;parse&lt;/code&gt; throw.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L52)
