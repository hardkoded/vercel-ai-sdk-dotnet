---
title: "Vercel.AI.AspNetCore"
description: "UI message stream responses for ASP.NET Core."
editUrl: false
sidebar:
  label: "Vercel.AI.AspNetCore"
---

[API reference](/api/)

UI message stream responses for ASP.NET Core.

## Classes

| Type | Summary |
| --- | --- |
| [CreateAgentUIStreamOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptions/) | Options of CancellationToken). Maps to the options of createAgentUIStream. |
| [CreateAgentUIStreamOptions&lt;TCallOptions&gt;](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptions-1/) | Options of CancellationToken). Maps to the options of createAgentUIStream with a CALLOPTIONS type. |
| [CreateAgentUIStreamOptionsBase](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptionsBase/) | The options of createAgentUIStream that do not depend on the agent. The .NET split of the generic CALLOPTIONS parameter. |
| [CreateAgentUIStreamResponseOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamResponseOptions/) | Options of CancellationToken). Maps to the options of createAgentUIStreamResponse. |
| [CreateAgentUIStreamResponseOptions&lt;TCallOptions&gt;](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamResponseOptions-1/) | Options of CancellationToken). Maps to the options of createAgentUIStreamResponse with a CALLOPTIONS type. |
| [PipeAgentUIStreamToResponseOptions](/api/Vercel.AI.AspNetCore/PipeAgentUIStreamToResponseOptions/) | Options of CancellationToken). Maps to the options of pipeAgentUIStreamToResponse. |
| [PipeAgentUIStreamToResponseOptions&lt;TCallOptions&gt;](/api/Vercel.AI.AspNetCore/PipeAgentUIStreamToResponseOptions-1/) | Options of CancellationToken). Maps to the options of pipeAgentUIStreamToResponse with a CALLOPTIONS type. |
| [ReadUIMessageStreamOptions](/api/Vercel.AI.AspNetCore/ReadUIMessageStreamOptions/) | Options of CancellationToken). |
| [TextUIMessageStream](/api/Vercel.AI.AspNetCore/TextUIMessageStream/) | Turns plain text into the UI message chunks a text-only client expects. |
| [ToUIMessageChunkOptions](/api/Vercel.AI.AspNetCore/ToUIMessageChunkOptions/) | Options of ToUIMessageChunkOptions). Maps to ToUIMessageChunkOptions. |
| [UIMessageChunkException](/api/Vercel.AI.AspNetCore/UIMessageChunkException/) | Thrown when a UI message chunk does not match the protocol schema. |
| [UIMessageChunks](/api/Vercel.AI.AspNetCore/UIMessageChunks/) | Reads AI SDK UI message chunks, including fields added by a newer server. |
| [UIMessageChunkStreamResult](/api/Vercel.AI.AspNetCore/UIMessageChunkStreamResult/) | SSE response for UI message chunks that are already built. |
| [UIMessageStreamConverter](/api/Vercel.AI.AspNetCore/UIMessageStreamConverter/) | Converts streamText parts into AI SDK UI message chunks. |
| [UIMessageStreamExtensions](/api/Vercel.AI.AspNetCore/UIMessageStreamExtensions/) | Writes a StreamTextResult as an AI SDK UI message stream. |
| [UIMessageStreamOnEndEvent](/api/Vercel.AI.AspNetCore/UIMessageStreamOnEndEvent/) | The event that OnEnd receives. Maps to UIMessageStreamOnEndCallback's event. |
| [UIMessageStreamOnStepEndEvent](/api/Vercel.AI.AspNetCore/UIMessageStreamOnStepEndEvent/) | The event that OnStepEnd receives. Maps to UIMessageStreamOnStepEndCallback's event. |
| [UIMessageStreamOptions](/api/Vercel.AI.AspNetCore/UIMessageStreamOptions/) | Options of CancellationToken). Maps to UIMessageStreamOptions. |
| [UIMessageStreamOutcome](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcome/) | The operation-level outcome of a UI message stream. This is separate from model finish reasons and individual stream chunks. Fatal stream-processing failures override outcomes declared by the stream owner. Consumer cancellation before an outcome is declared keeps the Unknown status and is reported separately through IsCancelled. Maps to UIMessageStreamOutcome. |
| [UIMessageStreamResponse](/api/Vercel.AI.AspNetCore/UIMessageStreamResponse/) | Response whose body is a UI message stream encoded as Server-Sent Events. |
| [UIMessageStreamResponseInit](/api/Vercel.AI.AspNetCore/UIMessageStreamResponseInit/) | Options for a UI message stream response. Maps to UIMessageStreamResponseInit. |
| [UIMessageStreamResponses](/api/Vercel.AI.AspNetCore/UIMessageStreamResponses/) | Builds UI message stream responses. Maps to createUIMessageStreamResponse. |
| [UIMessageStreamResult](/api/Vercel.AI.AspNetCore/UIMessageStreamResult/) | SSE result for the AI SDK UI message protocol. |

## Enums

| Type | Summary |
| --- | --- |
| [UIMessageStreamOutcomeStatus](/api/Vercel.AI.AspNetCore/UIMessageStreamOutcomeStatus/) | The status of a UIMessageStreamOutcome. |
