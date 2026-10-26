---
title: "OpenAICompatibleImages"
description: "Shared image request helpers."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleImages"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public static class OpenAICompatibleImages
```


Shared image request helpers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L931)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages_FileContent_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages_FileContent_Vercel_AI_Operations_ImageModelFile_"></a>

### FileContent(ImageModelFile)


```csharp
public static ByteArrayContent FileContent(ImageModelFile file)
```


Multipart content for one image file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/) |  |

**Returns:** [ByteArrayContent](https://learn.microsoft.com/dotnet/api/system.net.http.bytearraycontent)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L951)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages_MergeOptions_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages_MergeOptions_System_Text_Json_Nodes_JsonObject_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Collections_Generic_IList_Vercel_AI_Provider_CallWarning__"></a>

### `MergeOptions(JsonObject, string, IReadOnlyDictionary<string, JsonElement>?, IList<CallWarning>)`


```csharp
public static void MergeOptions(JsonObject body, string providerId, IReadOnlyDictionary<string, JsonElement>? providerOptions, IList<CallWarning> warnings)
```


Merges provider options into an image body. Camel-case keys win.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `warnings` | [IList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L934)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages_ReadInt_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImages_ReadInt_System_Text_Json_JsonElement_System_String_"></a>

### ReadInt(JsonElement, string)


```csharp
public static int? ReadInt(JsonElement obj, string name)
```


Reads an optional integer field.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `obj` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L960)
