// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>An agent that calls tools until <see cref="StopWhen"/> matches. Maps to <c>Agent</c>.</summary>
public sealed class Agent
{
    private readonly IAiClient _client;

    /// <summary>Creates an agent. The default stop condition is 10 steps.</summary>
    public Agent(AgentOptions options, IAiClient? client = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options;
        _client = client ?? new AiClient(Vercel.AI.Gateway.GatewayProvider.Create());
    }

    /// <summary>Agent options.</summary>
    public AgentOptions Options { get; }

    /// <summary>Generates a reply to <paramref name="prompt"/>.</summary>
    public Task<GenerateTextResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return _client.GenerateTextAsync(Prepare(CreateOptions(prompt)), cancellationToken);
    }

    /// <summary>Streams a reply to <paramref name="prompt"/>. <paramref name="abortSignal"/> aborts the stream and carries the reason.</summary>
    public StreamTextResult Stream(string prompt, CancellationToken cancellationToken = default, Vercel.AI.Util.AbortSignal? abortSignal = null)
    {
        var prepared = Prepare(CreateOptions(prompt));
        return _client.StreamTextAsync(new StreamTextOptions
        {
            Model = prepared.Model,
            ModelId = prepared.ModelId,
            System = prepared.System,
            Instructions = prepared.Instructions,
            Prompt = prepared.Prompt,
            Messages = prepared.Messages,
            Tools = prepared.Tools,
            ActiveTools = prepared.ActiveTools,
            ToolCallConcurrency = prepared.ToolCallConcurrency,
            ToolChoice = prepared.ToolChoice,
            StopWhen = prepared.StopWhen,
            Output = prepared.Output,
            MaxOutputTokens = prepared.MaxOutputTokens,
            Temperature = prepared.Temperature,
            TopP = prepared.TopP,
            TopK = prepared.TopK,
            PresencePenalty = prepared.PresencePenalty,
            FrequencyPenalty = prepared.FrequencyPenalty,
            StopSequences = prepared.StopSequences,
            Seed = prepared.Seed,
            Reasoning = prepared.Reasoning,
            MaxRetries = prepared.MaxRetries,
            Timeout = prepared.Timeout,
            Headers = prepared.Headers,
            ProviderOptions = prepared.ProviderOptions,
            PrepareStep = prepared.PrepareStep,
            OnStepEnd = prepared.OnStepEnd,
            OnFinish = prepared.OnFinish,
            OnError = prepared.OnError,
            ApproveTool = prepared.ApproveTool,
#pragma warning disable CS0618
            RepairToolCall = prepared.RepairToolCall ?? prepared.ExperimentalRepairToolCall,
#pragma warning restore CS0618
            RefineToolInput = prepared.RefineToolInput,
            AbortSignal = abortSignal,
            OnAbort = Options.OnAbort,
        }, cancellationToken);
    }

    private GenerateTextOptions Prepare(GenerateTextOptions call)
    {
        return Options.PrepareCall?.Invoke(call) ?? call;
    }

    private GenerateTextOptions CreateOptions(string prompt)
    {
        return new GenerateTextOptions
        {
            Model = Options.Model,
            ModelId = Options.ModelId,
            Instructions = Options.Instructions,
            Prompt = prompt,
            Tools = Options.Tools,
            StopWhen = Options.StopWhen ?? StopWhen.CreateDefaultStopCondition(10),
            Temperature = Options.Temperature,
            MaxOutputTokens = Options.MaxOutputTokens,
            ToolCallConcurrency = Options.ToolCallConcurrency,
            RepairToolCall = Options.RepairToolCall,
            RefineToolInput = Options.RefineToolInput,
        };
    }
}

/// <summary>Agent settings.</summary>
public sealed class AgentOptions
{
    /// <summary>Explicit model.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>Gateway model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>System instructions: a string, one <see cref="SystemModelMessage"/>, or an array of them.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>Tools.</summary>
    public IReadOnlyList<Tool>? Tools { get; set; }

    /// <summary>Stop condition. Defaults to 10 steps.</summary>
    public StopCondition? StopWhen { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Maximum number of tool calls that may execute concurrently within a step. See <see cref="GenerateTextOptions.ToolCallConcurrency"/>.
    /// <see cref="PrepareCall"/> can replace it.
    /// </summary>
    public int? ToolCallConcurrency { get; set; }

    /// <summary>Repairs a tool call that failed to parse. See <see cref="GenerateTextOptions.RepairToolCall"/>. <see cref="PrepareCall"/> can replace it.</summary>
    public ToolCallRepairFunction? RepairToolCall { get; set; }

    /// <summary>Refines the parsed input of tool calls. See <see cref="GenerateTextOptions.RefineToolInput"/>. <see cref="PrepareCall"/> can replace it.</summary>
    public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; set; }

    /// <summary>
    /// Called before each <see cref="Agent.GenerateAsync"/> and <see cref="Agent.Stream"/> call with the options the agent would use,
    /// including the default <see cref="GenerateTextOptions.StopWhen"/>. Return replacement options, or null to keep them.
    /// A returned options object that sets <see cref="GenerateTextOptions.StopWhen"/> replaces the default limit. Maps to <c>prepareCall</c>.
    /// </summary>
    public Func<GenerateTextOptions, GenerateTextOptions?>? PrepareCall { get; set; }

    /// <summary>Called after <see cref="Agent.Stream"/> is aborted. See <see cref="StreamTextOptions.OnAbort"/>.</summary>
    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }
}

/// <summary>
/// Looks up provider models by <c>provider:model</c> ids. Maps to <c>createProviderRegistry</c>.
/// Everything after the first separator is the model id, so model ids may contain the separator.
/// </summary>
public sealed class ProviderRegistry
{
    private readonly Dictionary<string, ProviderBase> _providers = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _separator;

    /// <summary>Creates a registry. <paramref name="separator"/> splits the provider id from the model id.</summary>
    public ProviderRegistry(string separator = ":")
    {
        if (string.IsNullOrEmpty(separator))
        {
            throw new ArgumentException("A separator is required.", nameof(separator));
        }

        _separator = separator;
    }

    /// <summary>Middleware applied to every language model the registry returns. The first one sees the call first.</summary>
    public IReadOnlyList<ILanguageModelMiddleware>? LanguageModelMiddleware { get; set; }

    /// <summary>Registers a provider under <paramref name="id"/>.</summary>
    public void Register(string id, ProviderBase provider)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Provider id is required.", nameof(id));
        }

        _providers[id] = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>Resolves <c>provider:model</c> to a language model.</summary>
    public ILanguageModel LanguageModel(string providerAndModel)
    {
        var model = Model(providerAndModel, "languageModel", (provider, modelId) => provider.LanguageModel(modelId));
        return LanguageModelMiddleware is { Count: > 0 } middleware ? model.WrapLanguageModel(middleware.ToArray()) : model;
    }

    /// <summary>Resolves <c>provider:model</c> to an embedding model.</summary>
    public IEmbeddingModel EmbeddingModel(string providerAndModel)
    {
        return Model(providerAndModel, "embeddingModel", (provider, modelId) => provider.EmbeddingModel(modelId));
    }

    /// <summary>Resolves <c>provider:model</c> to an image model.</summary>
    public IImageModel ImageModel(string providerAndModel)
    {
        return Model(providerAndModel, "imageModel", (provider, modelId) => provider.ImageModel(modelId));
    }

    /// <summary>Resolves <c>provider:model</c> to a transcription model.</summary>
    public ITranscriptionModel TranscriptionModel(string providerAndModel)
    {
        return Model(providerAndModel, "transcriptionModel", (provider, modelId) => provider.TranscriptionModel(modelId));
    }

    /// <summary>Resolves <c>provider:model</c> to a speech model.</summary>
    public ISpeechModel SpeechModel(string providerAndModel)
    {
        return Model(providerAndModel, "speechModel", (provider, modelId) => provider.SpeechModel(modelId));
    }

    /// <summary>Resolves <c>provider:model</c> to a reranking model.</summary>
    public IRerankingModel RerankingModel(string providerAndModel)
    {
        return Model(providerAndModel, "rerankingModel", (provider, modelId) => provider.RerankingModel(modelId));
    }

    /// <summary>Resolves <c>provider:model</c> to a video model.</summary>
    public IVideoModel VideoModel(string providerAndModel)
    {
        return Model(providerAndModel, "videoModel", (provider, modelId) => provider.VideoModel(modelId));
    }

    /// <summary>Resolves <c>provider:model</c> to a model that scores a candidate against a rubric.</summary>
    public IRubricModel RubricModel(string providerAndModel)
    {
        return Model(providerAndModel, "rubricModel", (provider, modelId) => provider.RubricModel(modelId));
    }

    /// <summary>
    /// Resolves <c>provider:model</c> to a decision model. The provider must implement <see cref="Operations.IDecisionProvider"/>
    /// or the deprecated <see cref="Operations.IEvaluationProvider"/>, or <see cref="NoSuchModelError"/> is thrown.
    /// </summary>
    public Operations.IDecisionCaller DecisionModel(string providerAndModel)
    {
        return Operations.DecisionModels.Validate(Model(providerAndModel, "decisionModel", Operations.DecisionModels.FromProvider));
    }

    /// <summary>
    /// Deprecated alias of <see cref="DecisionModel"/>. Returns the same model, which also has the deprecated
    /// <see cref="Operations.IEvaluationCaller.DoEvaluateAsync"/>. Maps to <c>evaluationModel</c>.
    /// </summary>
    [Obsolete("Use DecisionModel instead.")]
    public Operations.IEvaluationDecisionCaller EvaluationModel(string providerAndModel)
    {
        return Operations.DecisionModels.AsEvaluationModel(DecisionModel(providerAndModel));
    }

    /// <summary>Returns the file store of the provider registered under <paramref name="providerId"/>.</summary>
    public IFileStore FileStore(string providerId)
    {
        return Provider(providerId, "languageModel").FileStore();
    }

    /// <summary>Returns the skill store of the provider registered under <paramref name="providerId"/>.</summary>
    public ISkillStore SkillStore(string providerId)
    {
        return Provider(providerId, "languageModel").SkillStore();
    }

    // A provider that returns null has no such model.
    private T Model<T>(string id, string modelType, Func<ProviderBase, string, T?> create)
        where T : class
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        var index = id.IndexOf(_separator, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new NoSuchModelError(id, modelType, "Invalid " + modelType + " id for registry: " + id + " (must be in the format \"providerId" + _separator + "modelId\")");
        }

        var provider = Provider(id.Substring(0, index), modelType);
        return create(provider, id.Substring(index + _separator.Length)) ?? throw new NoSuchModelError(id, modelType);
    }

    private ProviderBase Provider(string providerId, string modelType)
    {
        return _providers.TryGetValue(providerId, out var provider)
            ? provider
            : throw new NoSuchProviderError(providerId, modelType, providerId, _providers.Keys.ToList());
    }
}

/// <summary>
/// A provider backed by caller-supplied models. Maps to <c>customProvider</c>. A model id that is not registered goes to
/// <see cref="FallbackProvider"/>, or throws <see cref="NoSuchModelError"/> when there is none.
/// </summary>
public sealed class CustomProvider : ProviderBase, Operations.IDecisionProvider
{
    private readonly Dictionary<string, ILanguageModel> _language = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IEmbeddingModel> _embedding = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IImageModel> _image = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ITranscriptionModel> _transcription = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ISpeechModel> _speech = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IRerankingModel> _reranking = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IVideoModel> _video = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IRubricModel> _rubric = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<Operations.IDecisionCaller>> _decision = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<Operations.IDecisionCaller>> _legacyDecision = new(StringComparer.Ordinal);

    /// <summary>Creates a custom provider.</summary>
    public CustomProvider(string name = "custom")
        : base(name)
    {
    }

    /// <summary>Provider asked for model ids that are not registered here.</summary>
    public ProviderBase? FallbackProvider { get; set; }

    /// <summary>File store. When null, the fallback provider's file store is used.</summary>
    public IFileStore? Files { get; set; }

    /// <summary>Skill store. When null, the fallback provider's skill store is used.</summary>
    public ISkillStore? Skills { get; set; }

    /// <summary>Registers a language model under its own model id.</summary>
    public CustomProvider AddLanguageModel(ILanguageModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return AddLanguageModel(model.ModelId, model);
    }

    /// <summary>Registers a language model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddLanguageModel(string modelId, ILanguageModel model)
    {
        return Add(_language, modelId, model);
    }

    /// <summary>Registers an embedding model under its own model id.</summary>
    public CustomProvider AddEmbeddingModel(IEmbeddingModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return AddEmbeddingModel(model.ModelId, model);
    }

    /// <summary>Registers an embedding model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddEmbeddingModel(string modelId, IEmbeddingModel model)
    {
        return Add(_embedding, modelId, model);
    }

    /// <summary>Registers an image model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddImageModel(string modelId, IImageModel model)
    {
        return Add(_image, modelId, model);
    }

    /// <summary>Registers a transcription model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddTranscriptionModel(string modelId, ITranscriptionModel model)
    {
        return Add(_transcription, modelId, model);
    }

    /// <summary>Registers a speech model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddSpeechModel(string modelId, ISpeechModel model)
    {
        return Add(_speech, modelId, model);
    }

    /// <summary>Registers a reranking model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddRerankingModel(string modelId, IRerankingModel model)
    {
        return Add(_reranking, modelId, model);
    }

    /// <summary>Registers a video model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddVideoModel(string modelId, IVideoModel model)
    {
        return Add(_video, modelId, model);
    }

    /// <summary>Registers a model that scores a candidate against a rubric under <paramref name="modelId"/>.</summary>
    public CustomProvider AddRubricModel(string modelId, IRubricModel model)
    {
        return Add(_rubric, modelId, model);
    }

    /// <summary>Deprecated name of <see cref="AddRubricModel"/>. Registers a rubric model, not a decision model.</summary>
    [Obsolete("Use AddRubricModel instead.")]
    public CustomProvider AddEvaluationModel(string modelId, IEvaluationModel model)
    {
        return AddRubricModel(modelId, RubricModels.FromLegacy(model));
    }

    /// <summary>Registers a decision model under <paramref name="modelId"/>. Maps to <c>decisionModels</c>.</summary>
    public CustomProvider AddDecisionModel(string modelId, Operations.IDecisionCaller model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return Add(_decision, modelId, () => model);
    }

    /// <summary>
    /// Registers a string alias for a decision model. The id is resolved through
    /// <see cref="Operations.DecisionModels.DefaultProvider"/>, or Gateway when none is set, when <see cref="DecisionModel"/> is called.
    /// </summary>
    public CustomProvider AddDecisionModel(string modelId, string providerModelId)
    {
        if (providerModelId is null)
        {
            throw new ArgumentNullException(nameof(providerModelId));
        }

        return Add(_decision, modelId, () => Operations.DecisionModels.Resolve(null, providerModelId));
    }

    /// <summary>
    /// Registers a deprecated decision model under <paramref name="modelId"/>. Maps to <c>evaluationModels</c>.
    /// These models are ignored once any model is added with <c>AddDecisionModel</c>.
    /// </summary>
    [Obsolete("Use AddDecisionModel instead.")]
    public CustomProvider AddEvaluationModel(string modelId, Operations.IEvaluationCaller model)
    {
        var legacy = Operations.DecisionCaller.FromLegacy(model);
        return Add(_legacyDecision, modelId, () => legacy);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return Find(_language, modelId, "languageModel", fallback => fallback.LanguageModel(modelId));
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return Find(_embedding, modelId, "embeddingModel", fallback => fallback.EmbeddingModel(modelId));
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return Find(_image, modelId, "imageModel", fallback => fallback.ImageModel(modelId));
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return Find(_transcription, modelId, "transcriptionModel", fallback => fallback.TranscriptionModel(modelId));
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return Find(_speech, modelId, "speechModel", fallback => fallback.SpeechModel(modelId));
    }

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId)
    {
        return Find(_reranking, modelId, "rerankingModel", fallback => fallback.RerankingModel(modelId));
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId)
    {
        return Find(_video, modelId, "videoModel", fallback => fallback.VideoModel(modelId));
    }

    /// <inheritdoc />
    public override IRubricModel RubricModel(string modelId)
    {
        return Find(_rubric, modelId, "rubricModel", fallback => fallback.RubricModel(modelId));
    }

    /// <summary>
    /// Returns the decision model registered under <paramref name="modelId"/>, or asks <see cref="FallbackProvider"/>
    /// when it implements <see cref="Operations.IDecisionProvider"/> or the deprecated <see cref="Operations.IEvaluationProvider"/>.
    /// </summary>
    public Operations.IDecisionCaller DecisionModel(string modelId)
    {
        var models = _decision.Count > 0 ? _decision : _legacyDecision;
        var factory = Find(models, modelId, "decisionModel", fallback => Operations.DecisionModels.FromProvider(fallback, modelId) is { } found ? () => found : null);
        return Operations.DecisionModels.Validate(factory());
    }

    /// <summary>
    /// Deprecated alias of <see cref="DecisionModel"/>. Returns the same model, which also has the deprecated
    /// <see cref="Operations.IEvaluationCaller.DoEvaluateAsync"/>. Maps to <c>evaluationModel</c>.
    /// </summary>
    [Obsolete("Use DecisionModel instead.")]
    public Operations.IEvaluationDecisionCaller EvaluationModel(string modelId)
    {
        return Operations.DecisionModels.AsEvaluationModel(DecisionModel(modelId));
    }

    /// <inheritdoc />
    public override IFileStore FileStore()
    {
        return Files ?? FallbackProvider?.FileStore() ?? base.FileStore();
    }

    /// <inheritdoc />
    public override ISkillStore SkillStore()
    {
        return Skills ?? FallbackProvider?.SkillStore() ?? base.SkillStore();
    }

    private CustomProvider Add<T>(Dictionary<string, T> models, string modelId, T model)
    {
        if (modelId is null)
        {
            throw new ArgumentNullException(nameof(modelId));
        }

        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        models[modelId] = model;
        return this;
    }

    // A fallback that returns null has no such model.
    private T Find<T>(Dictionary<string, T> models, string modelId, string modelType, Func<ProviderBase, T?> fallback)
        where T : class
    {
        if (models.TryGetValue(modelId, out var model))
        {
            return model;
        }

        return (FallbackProvider == null ? null : fallback(FallbackProvider)) ?? throw new NoSuchModelError(modelId, modelType);
    }
}
