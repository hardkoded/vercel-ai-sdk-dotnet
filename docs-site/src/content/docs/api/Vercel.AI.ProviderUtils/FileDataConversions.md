---
title: "FileDataConversions"
description: "Converts file payloads to the forms providers send."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileDataConversions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_FileDataConversions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class FileDataConversions
```


Converts file payloads to the forms providers send.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/FileDataConversions.cs#L12)

## Methods

<a id="Vercel_AI_ProviderUtils_FileDataConversions_ConvertImageModelFileToDataUri_"></a>
<a id="Vercel_AI_ProviderUtils_FileDataConversions_ConvertImageModelFileToDataUri_Vercel_AI_Operations_ImageModelFile_"></a>

### ConvertImageModelFileToDataUri(ImageModelFile)


```csharp
public static string ConvertImageModelFileToDataUri(ImageModelFile file)
```


Returns the URL of a URL file, or a base64 data URI for a file with bytes.
Maps to `convertImageModelFileToDataUri`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/FileDataConversions.cs#L40)

<a id="Vercel_AI_ProviderUtils_FileDataConversions_ConvertInlineFileDataToUint8Array_"></a>
<a id="Vercel_AI_ProviderUtils_FileDataConversions_ConvertInlineFileDataToUint8Array_Vercel_AI_Prompt_LanguageModelFilePart_"></a>

### ConvertInlineFileDataToUint8Array(LanguageModelFilePart)


```csharp
public static byte[] ConvertInlineFileDataToUint8Array(LanguageModelFilePart part)
```


Returns the bytes of an inline `data` or `text` file part. Base64 data is decoded and text is UTF-8 encoded.
Maps to `convertInlineFileDataToUint8Array`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `part` | [LanguageModelFilePart](/api/Vercel.AI.Prompt/LanguageModelFilePart/) |  |

**Returns:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/FileDataConversions.cs#L18)
