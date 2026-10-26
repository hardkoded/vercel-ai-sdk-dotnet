---
title: "SecureJson"
description: "Parses JSON and rejects proto and constructor.prototype payload keys."
editUrl: false
prev: false
next: false
sidebar:
  label: "SecureJson"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SecureJson"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class SecureJson
```


Parses JSON and rejects `__proto__` and `constructor.prototype` payload keys.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L77)

## Methods

<a id="Vercel_AI_ProviderUtils_SecureJson_Parse_"></a>
<a id="Vercel_AI_ProviderUtils_SecureJson_Parse_System_String_"></a>

### Parse(string)


```csharp
public static JsonElement Parse(string text)
```


Parses &lt;code class="paramref"&gt;text&lt;/code&gt; and returns a detached element.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L90)
