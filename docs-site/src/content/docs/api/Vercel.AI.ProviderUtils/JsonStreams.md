---
title: "JsonStreams"
description: "Reads JSON and Server-Sent Event bodies, including response headers."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonStreams"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonStreams"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonStreams
```


Reads JSON and Server-Sent Event bodies, including response headers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L581)

## Fields

<a id="Vercel_AI_ProviderUtils_JsonStreams_DefaultMaxLineBytes"></a>

### DefaultMaxLineBytes


```csharp
public const int DefaultMaxLineBytes = 67108864
```


Default largest JSON Lines row, in UTF-8 bytes.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L664)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonStreams_ExtractResponseHeaders_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_ExtractResponseHeaders_System_Net_Http_HttpResponseMessage_"></a>

### ExtractResponseHeaders(HttpResponseMessage)


```csharp
public static IReadOnlyDictionary<string, string> ExtractResponseHeaders(HttpResponseMessage response)
```


Lower-cases response and content header names and joins repeated values with a comma.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |

**Returns:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L584)

<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonAsync_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonAsync_System_Net_Http_HttpResponseMessage_System_Text_Json_Nodes_JsonNode_System_Threading_CancellationToken_"></a>

### ReadJsonAsync(HttpResponseMessage, JsonNode?, CancellationToken)


```csharp
public static Task<JsonStreams.JsonBody> ReadJsonAsync(HttpResponseMessage response, JsonNode? schema, CancellationToken cancellationToken = default)
```


Reads a JSON response. Unknown object properties are removed when the schema says so.
A body above [DefaultMaxBytes](/api/Vercel.AI.Util/Download/#Vercel_AI_Util_Download_DefaultMaxBytes) throws a [DownloadError](/api/Vercel.AI.Util/DownloadError/) before it is read.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;JsonStreams.JsonBody&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L626)

<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonEventsAsync_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonEventsAsync_System_IO_Stream_System_Text_Json_Nodes_JsonNode_System_Threading_CancellationToken_"></a>

### ReadJsonEventsAsync(Stream, JsonNode?, CancellationToken)


```csharp
public static IAsyncEnumerable<JsonParseResult> ReadJsonEventsAsync(Stream stream, JsonNode? schema = null, CancellationToken cancellationToken = default)
```


Parses a Server-Sent Event stream into JSON values. `data: [DONE]` is ignored.
Events are validated with &lt;code class="paramref"&gt;schema&lt;/code&gt; when it is supplied.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonParseResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L761)

<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonLinesAsync_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonLinesAsync_System_IO_Stream_System_Text_Json_Nodes_JsonNode_System_Action_System_Threading_CancellationToken_System_Int32_System_String_"></a>

### ReadJsonLinesAsync(Stream?, JsonNode?, Action?, CancellationToken, int, string)


```csharp
public static IAsyncEnumerable<JsonElement> ReadJsonLinesAsync(Stream? stream, JsonNode? schema = null, Action? onCancel = null, CancellationToken cancellationToken = default, int maxLineBytes = 67108864, string url = "")
```


Yields one JSON value per non-empty line. Lines may be split across reads.
A final line without a newline is still yielded. Early disposal cancels &lt;code class="paramref"&gt;onCancel&lt;/code&gt;.
A row longer than &lt;code class="paramref"&gt;maxLineBytes&lt;/code&gt; UTF-8 bytes (the newline excluded) throws a
[DownloadError](/api/Vercel.AI.Util/DownloadError/) for &lt;code class="paramref"&gt;url&lt;/code&gt; and cancels the body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `onCancel` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `maxLineBytes` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L672)
