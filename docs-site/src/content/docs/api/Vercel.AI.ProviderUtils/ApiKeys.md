---
title: "ApiKeys"
description: "Resolves an API key from an explicit value or an environment variable."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiKeys"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ApiKeys"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ApiKeys
```


Resolves an API key from an explicit value or an environment variable.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ApiKeys.cs#L10)

## Methods

<a id="Vercel_AI_ProviderUtils_ApiKeys_Combine_"></a>
<a id="Vercel_AI_ProviderUtils_ApiKeys_Combine_System_String_System_String_"></a>

### Combine(string, string)


```csharp
public static Uri Combine(string baseUrl, string relativePath)
```


Joins a base URL and a relative path.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `relativePath` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ApiKeys.cs#L67)

<a id="Vercel_AI_ProviderUtils_ApiKeys_Require_"></a>
<a id="Vercel_AI_ProviderUtils_ApiKeys_Require_System_String_System_String_System_String___"></a>

### Require(string?, string, params string[])


```csharp
public static string Require(string? apiKey, string environmentVariable, params string[] additionalEnvironmentVariables)
```


Returns the first non-empty key, or throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `environmentVariable` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `additionalEnvironmentVariables` | [string\[\]](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ApiKeys.cs#L13)

<a id="Vercel_AI_ProviderUtils_ApiKeys_RequireAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ApiKeys_RequireAsync_Vercel_AI_ProviderUtils_Resolvable_System_String__System_String_System_String___System_Threading_CancellationToken_"></a>

### `RequireAsync(Resolvable<string>?, string, string[]?, CancellationToken)`


```csharp
public static Task<string> RequireAsync(Resolvable<string>? apiKey, string environmentVariable, string[]? additionalEnvironmentVariables = null, CancellationToken cancellationToken = default)
```


Resolves &lt;code class="paramref"&gt;apiKey&lt;/code&gt; (a value, a task or a callback) and returns the first non-empty key,
or throws. Maps to `loadApiKey` after `resolveWith`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | `Resolvable&lt;string&gt;` |  |
| `environmentVariable` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `additionalEnvironmentVariables` | [string\[\]](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ApiKeys.cs#L53)
