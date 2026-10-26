---
title: "GoogleVertexAuth"
description: "Header construction for Vertex. Live Application Default Credentials are not contacted."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexAuth"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexAuth"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleVertexAuth
```


Header construction for Vertex. Live Application Default Credentials are not contacted.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L142)

## Methods

<a id="Vercel_AI_Google_GoogleVertexAuth_Bearer_"></a>
<a id="Vercel_AI_Google_GoogleVertexAuth_Bearer_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Bearer(string, IReadOnlyDictionary<string, string?>?)`


```csharp
public static IReadOnlyDictionary<string, string?> Bearer(string accessToken, IReadOnlyDictionary<string, string?>? headers = null)
```


Bearer header for an access token that the caller already obtained.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `accessToken` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L145)

<a id="Vercel_AI_Google_GoogleVertexAuth_Express_"></a>
<a id="Vercel_AI_Google_GoogleVertexAuth_Express_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Express(string, IReadOnlyDictionary<string, string?>?)`


```csharp
public static IReadOnlyDictionary<string, string?> Express(string apiKey, IReadOnlyDictionary<string, string?>? headers = null)
```


Express Mode sends the API key and does not add a bearer token.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertex.cs#L165)
