---
title: "JsonBinaryFieldScanResult"
description: "The body with extracted strings replaced by placeholders, and the extracted bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonBinaryFieldScanResult"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class JsonBinaryFieldScanResult
```


The body with extracted strings replaced by placeholders, and the extracted bytes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L28)

## Constructors

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Byte____"></a>

### `JsonBinaryFieldScanResult(string, IReadOnlyDictionary<string, byte[]>)`


```csharp
public JsonBinaryFieldScanResult(string text, IReadOnlyDictionary<string, byte[]> binaries)
```


Creates a scan result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `binaries` | [IReadOnlyDictionary&lt;string, byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L31)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult_Binaries_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult_Binaries"></a>

### Binaries


```csharp
public IReadOnlyDictionary<string, byte[]> Binaries { get; }
```


The extracted bytes by the placeholder text that stands in for them.

**Value:** [IReadOnlyDictionary&lt;string, byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L41)

<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult_Text_"></a>
<a id="Vercel_AI_ProviderUtils_JsonBinaryFieldScanResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


The body with extracted strings replaced by placeholders.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L38)
