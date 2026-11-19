---
title: "VercelRequestContext"
description: "The ambient Vercel request context. Maps to the @vercel/request-context object that Vercel stores on globalThis[Symbol.for('@vercel/request-context')] and th..."
editUrl: false
prev: false
next: false
sidebar:
  label: "VercelRequestContext"
  hidden: true
---

<a id="Vercel_AI_Gateway_VercelRequestContext"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class VercelRequestContext
```


The ambient Vercel request context. Maps to the `@vercel/request-context` object that Vercel stores on
`globalThis[Symbol.for('@vercel/request-context')]` and that `getContext` in `@vercel/oidc` reads.
A .NET host supplies it with `String%7d)`, for example from ASP.NET Core middleware.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/VercelEnvironment.cs#L14)

## Methods

<a id="Vercel_AI_Gateway_VercelRequestContext_Use_"></a>
<a id="Vercel_AI_Gateway_VercelRequestContext_Use_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Use(IReadOnlyDictionary<string, string?>?)`


```csharp
public static IDisposable Use(IReadOnlyDictionary<string, string?>? headers)
```


Makes &lt;code class="paramref"&gt;headers&lt;/code&gt; the request headers of the current async flow (for example `x-vercel-id` and
`x-vercel-oidc-token`) until the returned scope is disposed. Header names are matched case-insensitively;
names that differ only by case must carry the same value, otherwise [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception) is thrown.
Disposing the scope also revokes the headers for work that captured them, and disposing twice does nothing.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/VercelEnvironment.cs#L26)

<a id="Vercel_AI_Gateway_VercelRequestContext_Use_System_Func_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String___"></a>

### `Use(Func<IReadOnlyDictionary<string, string?>?>?)`


```csharp
public static IDisposable Use(Func<IReadOnlyDictionary<string, string?>?>? get)
```


Makes &lt;code class="paramref"&gt;get&lt;/code&gt; the provider of the request headers of the current async flow, until the returned
scope is disposed. Maps to the `get` method of the `@vercel/request-context` object. The function runs
on every lookup. A null &lt;code class="paramref"&gt;get&lt;/code&gt; is a context without a `get` method, and a function that
returns null is a context without headers: both read as no headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `get` | [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/VercelEnvironment.cs#L57)
