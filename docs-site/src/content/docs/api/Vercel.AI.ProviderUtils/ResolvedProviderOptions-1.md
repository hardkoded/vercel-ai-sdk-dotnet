---
title: "ResolvedProviderOptions<T>"
description: "Result of Func%7b%60%600%2c%60%600%2c%60%600%7d)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ResolvedProviderOptions<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed record ResolvedProviderOptions<T> : IEquatable<ResolvedProviderOptions<T>>
```


Result of `Func%7b%60%600%2c%60%600%2c%60%600%7d)`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` | Parsed options type. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEquatable&lt;ResolvedProviderOptions&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L31)

## Constructors

<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1__ctor__0_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `ResolvedProviderOptions(T?, IReadOnlyList<string>)`


```csharp
public ResolvedProviderOptions(T? Options, IReadOnlyList<string> Namespaces)
```


Result of `Func%7b%60%600%2c%60%600%2c%60%600%7d)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `Options` | `T` | The resolved options, or `default` when no namespace is present. |
| `Namespaces` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | The namespaces that contributed to &lt;code class="paramref"&gt;Options&lt;/code&gt;, in resolution order. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L31)

## Properties

<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1_Namespaces_"></a>
<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1_Namespaces"></a>

### Namespaces


```csharp
public IReadOnlyList<string> Namespaces { get; init; }
```


The namespaces that contributed to &lt;code class="paramref"&gt;Options&lt;/code&gt;, in resolution order.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L31)

<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1_Options_"></a>
<a id="Vercel_AI_ProviderUtils_ResolvedProviderOptions_1_Options"></a>

### Options


```csharp
public T? Options { get; init; }
```


The resolved options, or `default` when no namespace is present.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L31)
