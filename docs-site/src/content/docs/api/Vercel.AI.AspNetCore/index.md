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
| [TextUIMessageStream](/api/Vercel.AI.AspNetCore/TextUIMessageStream/) | Turns plain text into the UI message chunks a text-only client expects. |
| [UIMessageChunkException](/api/Vercel.AI.AspNetCore/UIMessageChunkException/) | Thrown when a UI message chunk does not match the protocol schema. |
| [UIMessageChunks](/api/Vercel.AI.AspNetCore/UIMessageChunks/) | Reads AI SDK UI message chunks, including fields added by a newer server. |
| [UIMessageChunkStreamResult](/api/Vercel.AI.AspNetCore/UIMessageChunkStreamResult/) | SSE response for UI message chunks that are already built. |
| [UIMessageStreamExtensions](/api/Vercel.AI.AspNetCore/UIMessageStreamExtensions/) | Writes a StreamTextResult as an AI SDK UI message stream. |
| [UIMessageStreamResult](/api/Vercel.AI.AspNetCore/UIMessageStreamResult/) | SSE result for the AI SDK UI message protocol. |
