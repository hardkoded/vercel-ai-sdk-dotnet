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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L620)

## Fields

<a id="Vercel_AI_ProviderUtils_JsonStreams_DefaultMaxLineBytes"></a>

### DefaultMaxLineBytes


```csharp
public const int DefaultMaxLineBytes = 67108864
```


Default largest JSON Lines row, in UTF-8 bytes.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L762)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L623)

<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonAsync_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_ReadJsonAsync_System_Net_Http_HttpResponseMessage_System_Text_Json_Nodes_JsonNode_System_Threading_CancellationToken_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `ReadJsonAsync(HttpResponseMessage, JsonNode?, CancellationToken, IReadOnlyList<string>?)`


```csharp
public static Task<JsonStreams.JsonBody> ReadJsonAsync(HttpResponseMessage response, JsonNode? schema, CancellationToken cancellationToken = default, IReadOnlyList<string>? experimentalBinaryFields = null)
```


Reads a JSON response. Unknown object properties are removed when the schema says so.
A body above [DefaultMaxBytes](/api/Vercel.AI.Util/Download/#Vercel_AI_Util_Download_DefaultMaxBytes) throws a [DownloadError](/api/Vercel.AI.Util/DownloadError/) before it is read.
&lt;code class="paramref"&gt;experimentalBinaryFields&lt;/code&gt; are the paths of string fields that carry large base64 data, such as `images[*]`
or `content[*].data.data`. A path is a sequence of plain object keys joined by `.`, with `[*]` for every element of an array.
By default the whole body is buffered, decoded to text, parsed, and the base64 string is decoded again later.
With paths the body is read as a stream and canonical base64 strings of at least 64 KiB at those paths are decoded to bytes while
they arrive. [Value](/api/Vercel.AI.ProviderUtils/JsonStreams.JsonBody/#Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Value) and [RawValue](/api/Vercel.AI.ProviderUtils/JsonStreams.JsonBody/#Vercel_AI_ProviderUtils_JsonStreams_JsonBody_RawValue) hold a short placeholder string such as
`[binary data: 2097152 bytes; ref ...]` in their place, and [Binaries](/api/Vercel.AI.ProviderUtils/JsonStreams.JsonBody/#Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Binaries) maps the placeholder to the bytes.
Other strings, and strings at other paths, are parsed as usual. Maps to `createJsonResponseHandler`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `experimentalBinaryFields` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [Task&lt;JsonStreams.JsonBody&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | A path in &lt;code class="paramref"&gt;experimentalBinaryFields&lt;/code&gt; is not valid. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L687)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L871)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L770)
