// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

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
        return GenerateAsync(new AgentCall { Prompt = prompt }, cancellationToken);
    }

    /// <summary>Generates a reply. Call callbacks run after the callbacks on <see cref="AgentOptions"/>.</summary>
    public Task<GenerateTextResult> GenerateAsync(AgentCall call, CancellationToken cancellationToken = default)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        return _client.GenerateTextAsync(CreateOptions(call), cancellationToken);
    }

    /// <summary>Streams a reply to <paramref name="prompt"/>.</summary>
    public StreamTextResult Stream(string prompt, CancellationToken cancellationToken = default)
    {
        return Stream(new AgentCall { Prompt = prompt }, cancellationToken);
    }

    /// <summary>Streams a reply. Call callbacks run after the callbacks on <see cref="AgentOptions"/>.</summary>
    public StreamTextResult Stream(AgentCall call, CancellationToken cancellationToken = default)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var options = CreateOptions(call);
        return _client.StreamTextAsync(new StreamTextOptions
        {
            Model = options.Model,
            ModelId = options.ModelId,
            Instructions = options.Instructions,
            Prompt = options.Prompt,
            Messages = options.Messages,
            Tools = options.Tools,
            StopWhen = options.StopWhen,
            Temperature = options.Temperature,
            MaxOutputTokens = options.MaxOutputTokens,
            TopP = options.TopP,
            TopK = options.TopK,
            PresencePenalty = options.PresencePenalty,
            FrequencyPenalty = options.FrequencyPenalty,
            StopSequences = options.StopSequences,
            Seed = options.Seed,
            Headers = options.Headers,
            Reasoning = options.Reasoning,
            OnStepStart = options.OnStepStart,
            OnStepEnd = options.OnStepEnd,
            OnFinish = options.OnFinish,
            OnError = options.OnError,
        }, cancellationToken);
    }

    private GenerateTextOptions CreateOptions(AgentCall call)
    {
        return new GenerateTextOptions
        {
            Model = Options.Model,
            ModelId = Options.ModelId,
            Instructions = Options.Instructions,
            Prompt = call.Prompt,
            Messages = call.Messages,
            Tools = Options.Tools,
            StopWhen = Options.StopWhen ?? StopWhen.IsStepCount(10),
            Temperature = Options.Temperature,
            MaxOutputTokens = Options.MaxOutputTokens,
            TopP = Options.TopP,
            TopK = Options.TopK,
            PresencePenalty = Options.PresencePenalty,
            FrequencyPenalty = Options.FrequencyPenalty,
            StopSequences = Options.StopSequences,
            Seed = Options.Seed,
            Headers = Options.Headers,
            Reasoning = Options.Reasoning,
            OnStepStart = Chain(StepStart(Options.OnStepStart), StepStart(call.OnStepStart)),
            OnStepEnd = Chain(StepCallback(Options.OnStepEnd, Options.OnStepFinish), StepCallback(call.OnStepEnd, call.OnStepFinish)),
            OnFinish = Chain(Options.OnFinish, call.OnFinish),
            OnError = Chain(Options.OnError, call.OnError),
        };
    }

    private static Func<StepResult, CancellationToken, Task>? StepCallback(
        Func<StepResult, CancellationToken, Task>? onStepEnd,
        Func<StepResult, CancellationToken, Task>? onStepFinish)
    {
        return onStepEnd ?? onStepFinish;
    }

    private static Func<PrepareStepContext, CancellationToken, Task>? StepStart(Func<PrepareStepContext, CancellationToken, Task>? callback)
    {
        return callback;
    }

    private static Func<T, CancellationToken, Task>? Chain<T>(Func<T, CancellationToken, Task>? first, Func<T, CancellationToken, Task>? second)
    {
        if (first == null)
        {
            return second;
        }

        if (second == null)
        {
            return first;
        }

        return async (value, cancellationToken) =>
        {
            await first(value, cancellationToken).ConfigureAwait(false);
            await second(value, cancellationToken).ConfigureAwait(false);
        };
    }
}

/// <summary>One agent call. Maps to the object passed to <c>agent.generate</c> and <c>agent.stream</c>.</summary>
public sealed class AgentCall
{
    /// <summary>User prompt.</summary>
    public string? Prompt { get; set; }

    /// <summary>Prompt messages. Used when <see cref="Prompt"/> is not set.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>Called before each model call, after the agent callback.</summary>
    public Func<PrepareStepContext, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>Called after each step, after the agent callback. Wins over <see cref="OnStepFinish"/>.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Called after each step when <see cref="OnStepEnd"/> is not set on the same object.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }

    /// <summary>Called when generation finishes, after the agent callback.</summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }

    /// <summary>Called when generation fails, after the agent callback.</summary>
    public Func<Exception, CancellationToken, Task>? OnError { get; set; }
}

/// <summary>Agent settings.</summary>
public sealed class AgentOptions
{
    /// <summary>Explicit model.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>Gateway model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>System instructions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Tools.</summary>
    public IReadOnlyList<Tool>? Tools { get; set; }

    /// <summary>Stop condition. Defaults to 10 steps.</summary>
    public StopCondition? StopWhen { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Top-p.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k.</summary>
    public int? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Stop sequences.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Extra headers forwarded to the language model.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Reasoning effort forwarded to the language model.</summary>
    public string? Reasoning { get; set; }

    /// <summary>Called before each model call.</summary>
    public Func<PrepareStepContext, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>Called after each step. Wins over <see cref="OnStepFinish"/>.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Called after each step when <see cref="OnStepEnd"/> is not set.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }

    /// <summary>Called when generation finishes.</summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }

    /// <summary>Called when generation fails.</summary>
    public Func<Exception, CancellationToken, Task>? OnError { get; set; }
}

/// <summary>Looks up provider models by <c>provider:model</c> ids. Maps to <c>createProviderRegistry</c>.</summary>
public sealed class ProviderRegistry
{
    private readonly Dictionary<string, ProviderBase> _providers = new(StringComparer.OrdinalIgnoreCase);

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
        var split = Split(providerAndModel);
        return _providers[split.Provider].LanguageModel(split.Model);
    }

    /// <summary>Resolves <c>provider:model</c> to an embedding model.</summary>
    public IEmbeddingModel EmbeddingModel(string providerAndModel)
    {
        var split = Split(providerAndModel);
        return _providers[split.Provider].EmbeddingModel(split.Model);
    }

    private (string Provider, string Model) Split(string providerAndModel)
    {
        if (string.IsNullOrWhiteSpace(providerAndModel))
        {
            throw new ArgumentException("A provider:model id is required.", nameof(providerAndModel));
        }

        var index = providerAndModel.IndexOf(':');
        if (index <= 0 || index == providerAndModel.Length - 1)
        {
            throw new AiSdkException("Expected a provider:model id, got '" + providerAndModel + "'.");
        }

        var provider = providerAndModel.Substring(0, index);
        if (!_providers.ContainsKey(provider))
        {
            throw new AiSdkException("Provider '" + provider + "' is not registered.");
        }

        return (provider, providerAndModel.Substring(index + 1));
    }
}

/// <summary>A provider backed by caller-supplied models. Maps to <c>customProvider</c>.</summary>
public sealed class CustomProvider : ProviderBase
{
    private readonly Dictionary<string, ILanguageModel> _language = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IEmbeddingModel> _embedding = new(StringComparer.Ordinal);

    /// <summary>Creates a custom provider.</summary>
    public CustomProvider(string name = "custom")
        : base(name)
    {
    }

    /// <summary>Registers a language model.</summary>
    public CustomProvider AddLanguageModel(ILanguageModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        _language[model.ModelId] = model;
        return this;
    }

    /// <summary>Registers an embedding model.</summary>
    public CustomProvider AddEmbeddingModel(IEmbeddingModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        _embedding[model.ModelId] = model;
        return this;
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        if (_language.TryGetValue(modelId, out var model))
        {
            return model;
        }

        throw new AiSdkException("Custom provider '" + Name + "' has no language model '" + modelId + "'.");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        if (_embedding.TryGetValue(modelId, out var model))
        {
            return model;
        }

        return base.EmbeddingModel(modelId);
    }
}
