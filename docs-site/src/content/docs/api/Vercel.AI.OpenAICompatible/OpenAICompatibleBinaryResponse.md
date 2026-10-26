---
title: "OpenAICompatibleBinaryResponse"
description: "Raw bytes returned by a provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleBinaryResponse"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleBinaryResponse
```


Raw bytes returned by a provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L991)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse__ctor_System_Byte___System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Int32_"></a>

### `OpenAICompatibleBinaryResponse(byte[], IReadOnlyDictionary<string, string>, int)`


```csharp
public OpenAICompatibleBinaryResponse(byte[] body, IReadOnlyDictionary<string, string> headers, int statusCode)
```


Creates a binary response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L994)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse_Body_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse_Body"></a>

### Body


```csharp
public byte[] Body { get; }
```


Response bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1002)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse_Headers_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1005)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse_StatusCode_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleBinaryResponse_StatusCode"></a>

### StatusCode


```csharp
public int StatusCode { get; }
```


HTTP status code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1008)
