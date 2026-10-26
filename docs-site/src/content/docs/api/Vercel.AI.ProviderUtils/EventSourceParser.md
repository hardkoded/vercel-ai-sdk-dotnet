---
title: "EventSourceParser"
description: "Incremental Server-Sent Events parser. A UTF-8 BOM is ignored at the start of the stream. A trailing carriage return is held until the next chunk so it can f..."
editUrl: false
prev: false
next: false
sidebar:
  label: "EventSourceParser"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_EventSourceParser"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class EventSourceParser
```


Incremental Server-Sent Events parser. A UTF-8 BOM is ignored at the start of the
stream. A trailing carriage return is held until the next chunk so it can form
a `\r\n` line ending. `[DONE]` is ordinary data.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L36)

## Methods

<a id="Vercel_AI_ProviderUtils_EventSourceParser_Feed_"></a>
<a id="Vercel_AI_ProviderUtils_EventSourceParser_Feed_System_String_System_Action_Vercel_AI_ProviderUtils_EventSourceMessage__System_Action_System_Int32__System_Action_System_String__System_Action_System_String__"></a>

### `Feed(string?, Action<EventSourceMessage>, Action<int>?, Action<string>?, Action<string>?)`


```csharp
public void Feed(string? chunk, Action<EventSourceMessage> onEvent, Action<int>? onRetry = null, Action<string>? onComment = null, Action<string>? onError = null)
```


Feeds the next text chunk. Callbacks run for events completed by this chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `onEvent` | [Action&lt;EventSourceMessage&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onRetry` | [Action&lt;int&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onComment` | [Action&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onError` | [Action&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L46)

<a id="Vercel_AI_ProviderUtils_EventSourceParser_Reset_"></a>
<a id="Vercel_AI_ProviderUtils_EventSourceParser_Reset_System_Boolean_System_Action_System_Int32__System_Action_System_String__System_Action_System_String__"></a>

### `Reset(bool, Action<int>?, Action<string>?, Action<string>?)`


```csharp
public void Reset(bool consume = false, Action<int>? onRetry = null, Action<string>? onComment = null, Action<string>? onError = null)
```


Clears parser state. When &lt;code class="paramref"&gt;consume&lt;/code&gt; is true, an incomplete
line is parsed first. That can report `retry`, comments, and errors.
It does not dispatch a data event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `consume` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `onRetry` | [Action&lt;int&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onComment` | [Action&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onError` | [Action&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/EventSourceParser.cs#L109)
