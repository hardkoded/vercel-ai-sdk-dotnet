---
title: "UrlSupport"
description: "Checks whether a model can read a URL natively. Maps to isUrlSupported."
editUrl: false
prev: false
next: false
sidebar:
  label: "UrlSupport"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_UrlSupport"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class UrlSupport
```


Checks whether a model can read a URL natively. Maps to `isUrlSupported`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/UrlSupport.cs#L10)

## Methods

<a id="Vercel_AI_ProviderUtils_UrlSupport_IsUrlSupported_"></a>
<a id="Vercel_AI_ProviderUtils_UrlSupport_IsUrlSupported_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Collections_Generic_IReadOnlyList_System_Text_RegularExpressions_Regex___"></a>

### `IsUrlSupported(string, string, IReadOnlyDictionary<string, IReadOnlyList<Regex>>)`


```csharp
public static bool IsUrlSupported(string mediaType, string url, IReadOnlyDictionary<string, IReadOnlyList<Regex>> supportedUrls)
```


True when a pattern registered for &lt;code class="paramref"&gt;mediaType&lt;/code&gt;, its `type/*` prefix, or `*` matches &lt;code class="paramref"&gt;url&lt;/code&gt;.
The media type is compared in lower case; the URL keeps its case, so patterns decide case sensitivity. A top-level-only media type such as `image` only matches `image/*`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `supportedUrls` | [IReadOnlyDictionary&lt;string, IReadOnlyList&lt;Regex&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/UrlSupport.cs#L16)
