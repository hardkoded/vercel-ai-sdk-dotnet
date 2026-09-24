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
        return _client.GenerateTextAsync(CreateOptions(prompt), cancellationToken);
    }

    /// <summary>Streams a reply to <paramref name="prompt"/>.</summary>
    public StreamTextResult Stream(string prompt, CancellationToken cancellationToken = default)
    {
        return _client.StreamTextAsync(new StreamTextOptions
        {
            Model = Options.Model,
            ModelId = Options.ModelId,
            Instructions = Options.Instructions,
            Prompt = prompt,
            Tools = Options.Tools,
            StopWhen = Options.StopWhen ?? StopWhen.IsStepCount(10),
            Temperature = Options.Temperature,
            MaxOutputTokens = Options.MaxOutputTokens,
        }, cancellationToken);
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
            StopWhen = Options.StopWhen ?? StopWhen.IsStepCount(10),
            Temperature = Options.Temperature,
            MaxOutputTokens = Options.MaxOutputTokens,
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
