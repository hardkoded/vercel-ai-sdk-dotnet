---
title: "Vercel.AI.AssemblyAI"
description: "Public types in the `Vercel.AI.AssemblyAI` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.AssemblyAI"
---

[API reference](/api/)

Public types in the `Vercel.AI.AssemblyAI` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [AssemblyAIError](/api/Vercel.AI.AssemblyAI/AssemblyAIError/) | Parses AssemblyAI error JSON. |
| [AssemblyAIErrorData](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorData/) | Parsed AssemblyAI error. AssemblyAI documents a flat { "error": "Invalid API key" } body for 4xx and 5xx responses. The nested { "error": { "message": "...", "code": 429 } } body is still read. |
| [AssemblyAIErrorParseResult](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorParseResult/) | Result of parsing an AssemblyAI error body. |
| [AssemblyAIFailedResponseHandler](/api/Vercel.AI.AssemblyAI/AssemblyAIFailedResponseHandler/) | Creates .NET API exceptions from AssemblyAI failed JSON responses. |
| [AssemblyAIOptions](/api/Vercel.AI.AssemblyAI/AssemblyAIOptions/) | AssemblyAI settings. |
| [AssemblyAIProvider](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider/) | AssemblyAI provider. |
| [AssemblyAIProvider.AssemblyAISpeechModel](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider.AssemblyAISpeechModel/) | AssemblyAI text-to-speech model. Requests are buffered, not streamed. |
| [AssemblyAIProvider.AssemblyAITranscriptionModel](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider.AssemblyAITranscriptionModel/) | Uploads audio, submits a transcript, and polls until it completes. |
| [AssemblyAIServiceCollectionExtensions](/api/Vercel.AI.AssemblyAI/AssemblyAIServiceCollectionExtensions/) | Registers AssemblyAI. |
| [AssemblyAISpeechModelOptions](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechModelOptions/) | Provider options for AssemblyAI speech generation, read from providerOptions.assemblyai. Maps to AssemblyAISpeechModelOptions of assemblyai-speech-model-options.ts. |
| [AssemblyAISpeechRequest](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechRequest/) | One AssemblyAI text-to-speech call. |
| [AssemblyAISpeechResponse](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechResponse/) | Metadata returned with synthesized AssemblyAI speech. |
| [AssemblyAISpeechResult](/api/Vercel.AI.AssemblyAI/AssemblyAISpeechResult/) | Synthesized AssemblyAI audio. |
| [AssemblyAITranscriptionRequest](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionRequest/) | Provider options and headers for one AssemblyAI transcription. |
| [AssemblyAITranscriptionResponse](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionResponse/) | Metadata returned with a transcription. |
| [AssemblyAITranscriptionResult](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionResult/) | AssemblyAI transcription, including word segments and audio-intelligence metadata. |
| [AssemblyAITranscriptionSegment](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionSegment/) | One transcribed word. |
