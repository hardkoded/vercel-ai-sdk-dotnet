---
title: "Vercel.AI.Mistral"
description: "Public types in the `Vercel.AI.Mistral` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Mistral"
---

[API reference](/api/)

Public types in the `Vercel.AI.Mistral` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [LiveKitMediaStream](/api/Vercel.AI.Mistral/LiveKitMediaStream/) | A caller-owned stream. Maps to the browser MediaStream. |
| [LiveKitRpcInvocationData](/api/Vercel.AI.Mistral/LiveKitRpcInvocationData/) | Maps to RpcInvocationData. |
| [MistralConversationEndpointCapabilities](/api/Vercel.AI.Mistral/MistralConversationEndpointCapabilities/) | Conversations API capabilities. Maps to capabilities.language.endpoints.conversation. |
| [MistralConversationLanguageModel](/api/Vercel.AI.Mistral/MistralConversationLanguageModel/) | Mistral Conversations API language model, with support for web search. Maps to MistralConversationLanguageModel in mistral-conversation-language-model.ts. |
| [MistralEmbeddingModel](/api/Vercel.AI.Mistral/MistralEmbeddingModel/) | Mistral text embedding model. Maps to MistralEmbeddingModel. |
| [MistralEmbeddingModelEntry](/api/Vercel.AI.Mistral/MistralEmbeddingModelEntry/) | Embedding model entry of the Mistral catalog. |
| [MistralEmbeddingModelOptions](/api/Vercel.AI.Mistral/MistralEmbeddingModelOptions/) | Mistral embedding provider options. Maps to MistralEmbeddingModelOptions. |
| [MistralLanguageCapabilities](/api/Vercel.AI.Mistral/MistralLanguageCapabilities/) | Language capabilities of a Mistral model. Maps to the catalog's LanguageCapabilities. |
| [MistralLanguageEndpointCapabilities](/api/Vercel.AI.Mistral/MistralLanguageEndpointCapabilities/) | Per-endpoint capabilities. Maps to capabilities.language.endpoints. |
| [MistralLanguageModelChatOptions](/api/Vercel.AI.Mistral/MistralLanguageModelChatOptions/) | Mistral language model provider options. Maps to MistralLanguageModelChatOptions. |
| [MistralLanguageModelEntry](/api/Vercel.AI.Mistral/MistralLanguageModelEntry/) | Language model entry of the Mistral catalog. Maps to MistralLanguageModelEntry. |
| [MistralModelCatalog](/api/Vercel.AI.Mistral/MistralModelCatalog/) | Mistral model catalog. Maps to mistral-model-catalog.ts. |
| [MistralModelEntry](/api/Vercel.AI.Mistral/MistralModelEntry/) | An entry of Entries. Maps to MistralModelEntry. Exactly the sections the model declares are set. |
| [MistralOptions](/api/Vercel.AI.Mistral/MistralOptions/) | Mistral settings. Maps to MistralProviderSettings. |
| [MistralProvider](/api/Vercel.AI.Mistral/MistralProvider/) | Mistral provider. OpenAI Chat Completions compatible at https://api.mistral.ai/v1. |
| [MistralProviderOptions](/api/Vercel.AI.Mistral/MistralProviderOptions/) | Known options accepted under providerOptions.mistral. Maps to mistralProviderOptions. |
| [MistralRealtimeFactory](/api/Vercel.AI.Mistral/MistralRealtimeFactory/) | Creates Mistral voice agent models and manages their sessions. Maps to MistralRealtimeFactory, built by createMistralRealtimeFactory. Reached through ExperimentalRealtime. |
| [MistralRealtimeInactivityOptions](/api/Vercel.AI.Mistral/MistralRealtimeInactivityOptions/) | Reminders during periods of silence. Maps to turn.inactivity. |
| [MistralRealtimeModel](/api/Vercel.AI.Mistral/MistralRealtimeModel/) | A managed Mistral voice agent with a provider-owned LiveKit transport. Maps to MistralRealtimeModel. It has no WebSocket: BuildUri() and ConnectAsync(CancellationToken) are unsupported. |
| [MistralRealtimeModelOptions](/api/Vercel.AI.Mistral/MistralRealtimeModelOptions/) | Mistral voice agent options. Maps to mistralRealtimeModelOptions. |
| [MistralRealtimeSession](/api/Vercel.AI.Mistral/MistralRealtimeSession/) | Credentials for one browser voice conversation. Maps to MistralRealtimeSession. |
| [MistralRealtimeSttOptions](/api/Vercel.AI.Mistral/MistralRealtimeSttOptions/) | Speech recognition settings of the managed voice agent. Maps to stt. |
| [MistralRealtimeTransport](/api/Vercel.AI.Mistral/MistralRealtimeTransport/) | LiveKit stays inside the Mistral provider and is loaded only on connection. Maps to MistralRealtimeTransport. |
| [MistralRealtimeTtsOptions](/api/Vercel.AI.Mistral/MistralRealtimeTtsOptions/) | Speech synthesis settings. sessionConfig.voice overrides VoiceId. Maps to tts. |
| [MistralRealtimeTurnDetection](/api/Vercel.AI.Mistral/MistralRealtimeTurnDetection/) | Speech recognition turn detection of the managed voice agent. Maps to stt.turnDetection. |
| [MistralRealtimeTurnOptions](/api/Vercel.AI.Mistral/MistralRealtimeTurnOptions/) | Interruptions and reminders. Maps to turn. |
| [MistralReasoningCapabilities](/api/Vercel.AI.Mistral/MistralReasoningCapabilities/) | Reasoning support of a model. Maps to the catalog's ReasoningCapabilities. |
| [MistralServiceCollectionExtensions](/api/Vercel.AI.Mistral/MistralServiceCollectionExtensions/) | Registers MistralProvider. |
| [MistralSpeechCapabilities](/api/Vercel.AI.Mistral/MistralSpeechCapabilities/) | Speech capabilities of a Mistral model. Maps to the catalog's SpeechCapabilities. |
| [MistralSpeechModel](/api/Vercel.AI.Mistral/MistralSpeechModel/) | Mistral text-to-speech model. Maps to MistralSpeechModel. |
| [MistralSpeechModelEntry](/api/Vercel.AI.Mistral/MistralSpeechModelEntry/) | Speech model entry of the Mistral catalog. Maps to getMistralSpeechModelEntry's result. |
| [MistralSpeechModelOptions](/api/Vercel.AI.Mistral/MistralSpeechModelOptions/) | Mistral speech options. Maps to mistralSpeechModelOptions. |
| [MistralTools](/api/Vercel.AI.Mistral/MistralTools/) | Provider-executed tools of the Mistral Conversations API. Maps to mistralTools in tool/index.ts. |
| [MistralTranscriptionCapabilities](/api/Vercel.AI.Mistral/MistralTranscriptionCapabilities/) | Transcription capabilities of a Mistral model. Maps to the catalog's TranscriptionCapabilities. |
| [MistralTranscriptionModel](/api/Vercel.AI.Mistral/MistralTranscriptionModel/) | Mistral speech-to-text model. Maps to MistralTranscriptionModel. |
| [MistralTranscriptionModelEntry](/api/Vercel.AI.Mistral/MistralTranscriptionModelEntry/) | Transcription model entry of the Mistral catalog. Maps to getMistralTranscriptionModelEntry's result. |
| [MistralTranscriptionModelOptions](/api/Vercel.AI.Mistral/MistralTranscriptionModelOptions/) | Mistral transcription options. Maps to mistralTranscriptionModelOptions. |
| [MistralTurnDetection](/api/Vercel.AI.Mistral/MistralTurnDetection/) | Mistral transcription turn detection settings. Maps to the turnDetection option. |

## Interfaces

| Type | Summary |
| --- | --- |
| [ILiveKitClient](/api/Vercel.AI.Mistral/ILiveKitClient/) | The part of the LiveKit client that MistralRealtimeTransport uses. Upstream loads the livekit-client module on connection; the port has no LiveKit client, so the host supplies one through this interface. Maps to the livekit-client module (Room and createLocalAudioTrack). |
| [ILiveKitLocalAudioTrack](/api/Vercel.AI.Mistral/ILiveKitLocalAudioTrack/) | An audio track the transport created and must stop. Maps to LocalAudioTrack. |
| [ILiveKitLocalParticipant](/api/Vercel.AI.Mistral/ILiveKitLocalParticipant/) | Maps to LocalParticipant. |
| [ILiveKitMediaElement](/api/Vercel.AI.Mistral/ILiveKitMediaElement/) | An HTML media element playing a remote track. Maps to HTMLMediaElement. |
| [ILiveKitMediaTrack](/api/Vercel.AI.Mistral/ILiveKitMediaTrack/) | A media track of a stream or a LiveKit local track. Maps to the browser MediaStreamTrack. |
| [ILiveKitRemoteParticipant](/api/Vercel.AI.Mistral/ILiveKitRemoteParticipant/) | Maps to RemoteParticipant. |
| [ILiveKitRemoteTrack](/api/Vercel.AI.Mistral/ILiveKitRemoteTrack/) | Maps to RemoteTrack. |
| [ILiveKitRoom](/api/Vercel.AI.Mistral/ILiveKitRoom/) | Maps to Room. The events map to RoomEvent. |
