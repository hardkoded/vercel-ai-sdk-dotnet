---
title: "PathSegments"
description: "Encodes provider-returned identifiers for URL paths."
editUrl: false
prev: false
next: false
sidebar:
  label: "PathSegments"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_PathSegments"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class PathSegments
```


Encodes provider-returned identifiers for URL paths.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/PathSegments.cs#L8)

## Methods

<a id="Vercel_AI_ProviderUtils_PathSegments_Encode_"></a>
<a id="Vercel_AI_ProviderUtils_PathSegments_Encode_System_String_"></a>

### Encode(string)


```csharp
public static string Encode(string value)
```


Encodes a raw identifier as one URL path segment, including standalone dot segments that URL parsers
would otherwise normalize. Pass unencoded values; for resource paths with intentional slashes, encode
each segment separately.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/PathSegments.cs#L15)
