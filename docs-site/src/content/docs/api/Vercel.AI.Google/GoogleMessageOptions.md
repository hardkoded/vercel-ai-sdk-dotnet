---
title: "GoogleMessageOptions"
description: "Settings for converting a prompt into Gemini contents."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleMessageOptions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleMessageOptions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleMessageOptions
```


Settings for converting a prompt into Gemini `contents`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L14)

## Properties

<a id="Vercel_AI_Google_GoogleMessageOptions_IncludeFunctionCallIds_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_IncludeFunctionCallIds"></a>

### IncludeFunctionCallIds


```csharp
public bool IncludeFunctionCallIds { get; set; }
```


When false, function call and response ids are omitted. Vertex omits them.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L29)

<a id="Vercel_AI_Google_GoogleMessageOptions_IsGemini3Model_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_IsGemini3Model"></a>

### IsGemini3Model


```csharp
public bool? IsGemini3Model { get; set; }
```


When true, unsigned Gemini 3 function calls receive the documented skip sentinel.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L23)

<a id="Vercel_AI_Google_GoogleMessageOptions_IsGemmaModel_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_IsGemmaModel"></a>

### IsGemmaModel


```csharp
public bool? IsGemmaModel { get; set; }
```


When true, system text is prepended to the first user turn and `systemInstruction` is omitted.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L20)

<a id="Vercel_AI_Google_GoogleMessageOptions_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; set; }
```


Model id used to detect Gemma and Gemini 3 behavior when the flags below are left unset.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L17)

<a id="Vercel_AI_Google_GoogleMessageOptions_ProviderOptionNames_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_ProviderOptionNames"></a>

### ProviderOptionNames


```csharp
public IReadOnlyList<string> ProviderOptionNames { get; set; }
```


Provider-option namespaces, in precedence order.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L32)

<a id="Vercel_AI_Google_GoogleMessageOptions_SupportsFunctionResponseParts_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_SupportsFunctionResponseParts"></a>

### SupportsFunctionResponseParts


```csharp
public bool? SupportsFunctionResponseParts { get; set; }
```


When true, multimodal tool results use `functionResponse.parts`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L26)

<a id="Vercel_AI_Google_GoogleMessageOptions_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleMessageOptions_Warnings"></a>

### Warnings


```csharp
public List<GoogleWarning> Warnings { get; }
```


Warnings produced while converting.

**Value:** [List&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L35)
