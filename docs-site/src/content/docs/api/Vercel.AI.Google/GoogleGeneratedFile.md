---
title: "GoogleGeneratedFile"
description: "A generated file, including thought images."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleGeneratedFile"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleGeneratedFile"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleGeneratedFile : GeneratedFile
```


A generated file, including thought images.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedFile](/api/Vercel.AI.Provider/GeneratedFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L506)

## Constructors

<a id="Vercel_AI_Google_GoogleGeneratedFile__ctor_"></a>
<a id="Vercel_AI_Google_GoogleGeneratedFile__ctor_System_Byte___System_String_System_Boolean_System_Nullable_System_Text_Json_JsonElement__"></a>

### GoogleGeneratedFile(byte[], string, bool, JsonElement?)


```csharp
public GoogleGeneratedFile(byte[] data, string mediaType, bool thought, JsonElement? providerMetadata)
```


Creates a file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `thought` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L509)

## Properties

<a id="Vercel_AI_Google_GoogleGeneratedFile_ProviderMetadata_"></a>
<a id="Vercel_AI_Google_GoogleGeneratedFile_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L520)

<a id="Vercel_AI_Google_GoogleGeneratedFile_Thought_"></a>
<a id="Vercel_AI_Google_GoogleGeneratedFile_Thought"></a>

### Thought


```csharp
public bool Thought { get; }
```


True when the file was part of the model's thoughts.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L517)
