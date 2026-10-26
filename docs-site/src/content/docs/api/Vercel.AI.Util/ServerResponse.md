---
title: "ServerResponse"
description: "Byte sink with status, headers, and optional write backpressure. Mirrors the subset of Node ServerResponse used by the text-stream helpers."
editUrl: false
prev: false
next: false
sidebar:
  label: "ServerResponse"
  hidden: true
---

<a id="Vercel_AI_Util_ServerResponse"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ServerResponse
```


Byte sink with status, headers, and optional write backpressure.
Mirrors the subset of Node `ServerResponse` used by the text-stream helpers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L13)

## Constructors

<a id="Vercel_AI_Util_ServerResponse__ctor_"></a>
<a id="Vercel_AI_Util_ServerResponse__ctor"></a>

### ServerResponse()


```csharp
public ServerResponse()
```


Creates an empty response.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L21)

## Properties

<a id="Vercel_AI_Util_ServerResponse_DrainCount_"></a>
<a id="Vercel_AI_Util_ServerResponse_DrainCount"></a>

### DrainCount


```csharp
public int DrainCount { get; }
```


How many times [SimulateDrain()](#Vercel_AI_Util_ServerResponse_SimulateDrain) ran.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L46)

<a id="Vercel_AI_Util_ServerResponse_EnableBackpressure_"></a>
<a id="Vercel_AI_Util_ServerResponse_EnableBackpressure"></a>

### EnableBackpressure


```csharp
public bool EnableBackpressure { get; set; }
```


When true, the first write succeeds and later writes return false until [SimulateDrain()](#Vercel_AI_Util_ServerResponse_SimulateDrain).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L49)

<a id="Vercel_AI_Util_ServerResponse_Ended_"></a>
<a id="Vercel_AI_Util_ServerResponse_Ended"></a>

### Ended


```csharp
public bool Ended { get; }
```


Whether [End()](#Vercel_AI_Util_ServerResponse_End) has run.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L43)

<a id="Vercel_AI_Util_ServerResponse_Flush_"></a>
<a id="Vercel_AI_Util_ServerResponse_Flush"></a>

### Flush


```csharp
public Action<ServerResponse>? Flush { get; set; }
```


Called after each successful `Byte%5b%5d)` with this response.

**Value:** [Action&lt;ServerResponse&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L52)

<a id="Vercel_AI_Util_ServerResponse_Headers_"></a>
<a id="Vercel_AI_Util_ServerResponse_Headers"></a>

### Headers


```csharp
public Dictionary<string, object> Headers { get; }
```


Lowercased headers. Repeated names become a string array.

**Value:** [Dictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L34)

<a id="Vercel_AI_Util_ServerResponse_StatusCode_"></a>
<a id="Vercel_AI_Util_ServerResponse_StatusCode"></a>

### StatusCode


```csharp
public int StatusCode { get; }
```


Status code passed to `String)`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L28)

<a id="Vercel_AI_Util_ServerResponse_StatusMessage_"></a>
<a id="Vercel_AI_Util_ServerResponse_StatusMessage"></a>

### StatusMessage


```csharp
public string StatusMessage { get; }
```


Status text passed to `String)`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L31)

<a id="Vercel_AI_Util_ServerResponse_WriteCallCount_"></a>
<a id="Vercel_AI_Util_ServerResponse_WriteCallCount"></a>

### WriteCallCount


```csharp
public int WriteCallCount { get; }
```


How many times `Byte%5b%5d)` was called.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L40)

<a id="Vercel_AI_Util_ServerResponse_WrittenChunks_"></a>
<a id="Vercel_AI_Util_ServerResponse_WrittenChunks"></a>

### WrittenChunks


```csharp
public List<byte[]> WrittenChunks { get; }
```


Chunks accepted by `Byte%5b%5d)`.

**Value:** [List&lt;byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L37)

## Methods

<a id="Vercel_AI_Util_ServerResponse_End_"></a>
<a id="Vercel_AI_Util_ServerResponse_End"></a>

### End()


```csharp
public void End()
```


Marks the response finished.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L161)

<a id="Vercel_AI_Util_ServerResponse_GetDecodedChunks_"></a>
<a id="Vercel_AI_Util_ServerResponse_GetDecodedChunks"></a>

### GetDecodedChunks()


```csharp
public List<string> GetDecodedChunks()
```


Decodes each written chunk as UTF-8.

**Returns:** [List&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L174)

<a id="Vercel_AI_Util_ServerResponse_SetHeaders_"></a>
<a id="Vercel_AI_Util_ServerResponse_SetHeaders_Vercel_AI_Util_HeaderCollection_"></a>

### SetHeaders(HeaderCollection?)


```csharp
public void SetHeaders(HeaderCollection? headers)
```


Stores headers, lowercasing names and grouping duplicates.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L55)

<a id="Vercel_AI_Util_ServerResponse_SimulateDrain_"></a>
<a id="Vercel_AI_Util_ServerResponse_SimulateDrain"></a>

### SimulateDrain()


```csharp
public void SimulateDrain()
```


Releases a write that returned false.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L124)

<a id="Vercel_AI_Util_ServerResponse_WaitForDrainAsync_"></a>
<a id="Vercel_AI_Util_ServerResponse_WaitForDrainAsync"></a>

### WaitForDrainAsync()


```csharp
public Task WaitForDrainAsync()
```


Completes when [SimulateDrain()](#Vercel_AI_Util_ServerResponse_SimulateDrain) runs, or at once when it already ran after the last held write.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L142)

<a id="Vercel_AI_Util_ServerResponse_WaitForEndAsync_"></a>
<a id="Vercel_AI_Util_ServerResponse_WaitForEndAsync"></a>

### WaitForEndAsync()


```csharp
public Task WaitForEndAsync()
```


Completes when [End()](#Vercel_AI_Util_ServerResponse_End) runs.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L168)

<a id="Vercel_AI_Util_ServerResponse_Write_"></a>
<a id="Vercel_AI_Util_ServerResponse_Write_System_Byte___"></a>

### Write(byte[])


```csharp
public bool Write(byte[] chunk)
```


Appends &lt;code class="paramref"&gt;chunk&lt;/code&gt;. Returns false when the caller should wait for drain.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L96)

<a id="Vercel_AI_Util_ServerResponse_WriteHead_"></a>
<a id="Vercel_AI_Util_ServerResponse_WriteHead_System_Int32_System_String_"></a>

### WriteHead(int, string?)


```csharp
public void WriteHead(int statusCode, string? statusText = null)
```


Sets the status code and, when provided, the status text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ServerResponse.cs#L86)
