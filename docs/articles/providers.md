# Providers

Native wire protocols have their own request mappers:

- Gateway (`AI_GATEWAY_API_KEY`) posts V4 call options to `/language-model`, `/embedding-model`, and `/image-model`.
- OpenAI speaks Chat Completions, the Responses API, embeddings, images, speech, and transcription.
- Anthropic speaks the Messages API. `AnthropicAwsProvider` points that body at the AWS external endpoint.
- Google speaks Gemini `generateContent`. `GoogleVertexProvider` changes the base URL and sends a bearer token.
- Azure OpenAI uses `api-key` and `/openai/deployments/{model}/chat/completions`.
- Amazon Bedrock signs a Converse request with Signature Version 4.
- Cohere speaks chat, embed, and rerank.
- OpenResponses posts to `{base}/responses`. The default base is the Gateway Open Responses route.

OpenAI-compatible providers (Alibaba, Groq, DeepSeek, Mistral, xAI, and the others) are thin wrappers over `Vercel.AI.OpenAICompatible`. They set the base URL, the provider id, and the environment variable.

Speech, transcription, image, video, and Voyage each have a package that calls that provider’s public HTTP API. Use `SpeechModel`, `TranscriptionModel`, `ImageModel`, `VideoModel`, or `EmbeddingModel` rather than `LanguageModel`.
