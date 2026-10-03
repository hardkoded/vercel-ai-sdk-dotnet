// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Testing;

/// <summary>Call options recorded by the mock language models.</summary>
public sealed class MockLanguageModelCallOptions
{
}

/// <summary>Text content returned by a mock generate result.</summary>
public sealed class MockTextContent
{
    /// <summary>Creates text content.</summary>
    public MockTextContent(string text)
    {
        Text = text;
    }

    /// <summary>Always <c>text</c>.</summary>
    public string Type
    {
        get { return "text"; }
    }

    /// <summary>Generated text.</summary>
    public string Text { get; }
}

/// <summary>Generate result stored in a scripted list.</summary>
public sealed class MockGenerateResult
{
    /// <summary>Creates a result whose only content is <paramref name="text"/>.</summary>
    public MockGenerateResult(string text)
    {
        Content = new MockTextContent[] { new MockTextContent(text) };
    }

    /// <summary>Generated content.</summary>
    public IReadOnlyList<MockTextContent> Content { get; }
}

/// <summary>One chunk of a mock language-model stream.</summary>
public sealed class MockStreamChunk
{
    /// <summary>Creates a chunk.</summary>
    public MockStreamChunk(string type, string? id = null, string? delta = null)
    {
        Type = type;
        Id = id;
        Delta = delta;
    }

    /// <summary>Chunk kind.</summary>
    public string Type { get; }

    /// <summary>Chunk id.</summary>
    public string? Id { get; }

    /// <summary>Text delta, when <see cref="Type"/> is <c>text-delta</c>.</summary>
    public string? Delta { get; }
}

/// <summary>Stream result stored in a scripted list.</summary>
public sealed class MockStreamResult
{
    /// <summary>Creates a result that yields text-start, text-delta, and text-end for <paramref name="text"/>.</summary>
    public MockStreamResult(string text)
    {
        Chunks = new MockStreamChunk[]
        {
            new MockStreamChunk("text-start", text),
            new MockStreamChunk("text-delta", text, text),
            new MockStreamChunk("text-end", text),
        };
    }

    /// <summary>Creates a result with explicit chunks.</summary>
    public MockStreamResult(IReadOnlyList<MockStreamChunk> chunks)
    {
        Chunks = chunks ?? Array.Empty<MockStreamChunk>();
    }

    /// <summary>Chunks in order.</summary>
    public IReadOnlyList<MockStreamChunk> Chunks { get; }
}

/// <summary>Embedding values passed to a mock embedding model.</summary>
public sealed class MockEmbedOptions
{
    /// <summary>Creates embed options.</summary>
    public MockEmbedOptions(IReadOnlyList<string>? values)
    {
        Values = values ?? Array.Empty<string>();
    }

    /// <summary>Input values.</summary>
    public IReadOnlyList<string> Values { get; }
}

/// <summary>Embedding result stored in a scripted list.</summary>
public sealed class MockEmbedResult
{
    /// <summary>Creates a result with one embedding.</summary>
    public MockEmbedResult(float[] embedding)
    {
        Embeddings = new float[][] { embedding };
    }

    /// <summary>Embeddings, one per input value.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }
}

/// <summary>Language model v2 test double. Array-backed results are returned in order, starting at the first entry.</summary>
public sealed class MockLanguageModelV2
{
    private readonly IReadOnlyList<MockGenerateResult>? _generateResults;
    private readonly IReadOnlyList<MockStreamResult>? _streamResults;
    private readonly Func<MockLanguageModelCallOptions?, Task<MockGenerateResult>>? _generate;
    private readonly Func<MockLanguageModelCallOptions?, Task<MockStreamResult>>? _stream;

    /// <summary>Creates a model. List results are consumed from the first entry.</summary>
    public MockLanguageModelV2(
        string provider = "mock-provider",
        string modelId = "mock-model-id",
        IReadOnlyList<MockGenerateResult>? doGenerate = null,
        IReadOnlyList<MockStreamResult>? doStream = null,
        Func<MockLanguageModelCallOptions?, Task<MockGenerateResult>>? generate = null,
        Func<MockLanguageModelCallOptions?, Task<MockStreamResult>>? stream = null)
    {
        Provider = provider;
        ModelId = modelId;
        _generateResults = doGenerate;
        _streamResults = doStream;
        _generate = generate;
        _stream = stream;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v2"; }
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Generate calls in order.</summary>
    public List<MockLanguageModelCallOptions> DoGenerateCalls { get; } = new List<MockLanguageModelCallOptions>();

    /// <summary>Stream calls in order.</summary>
    public List<MockLanguageModelCallOptions> DoStreamCalls { get; } = new List<MockLanguageModelCallOptions>();

    /// <summary>Returns the next scripted generate result.</summary>
    public Task<MockGenerateResult> DoGenerateAsync(MockLanguageModelCallOptions? options)
    {
        DoGenerateCalls.Add(options ?? new MockLanguageModelCallOptions());
        if (_generate != null)
        {
            return _generate(options);
        }

        if (_generateResults != null)
        {
            return Task.FromResult(_generateResults[DoGenerateCalls.Count - 1]);
        }

        throw new NotSupportedException("doGenerate is not implemented.");
    }

    /// <summary>Returns the next scripted stream result.</summary>
    public Task<MockStreamResult> DoStreamAsync(MockLanguageModelCallOptions? options)
    {
        DoStreamCalls.Add(options ?? new MockLanguageModelCallOptions());
        if (_stream != null)
        {
            return _stream(options);
        }

        if (_streamResults != null)
        {
            return Task.FromResult(_streamResults[DoStreamCalls.Count - 1]);
        }

        throw new NotSupportedException("doStream is not implemented.");
    }
}

/// <summary>Language model v3 test double. Array-backed results are returned in order, starting at the first entry.</summary>
public sealed class MockLanguageModelV3
{
    private readonly MockLanguageModelV2 _inner;

    /// <summary>Creates a model. List results are consumed from the first entry.</summary>
    public MockLanguageModelV3(
        string provider = "mock-provider",
        string modelId = "mock-model-id",
        IReadOnlyList<MockGenerateResult>? doGenerate = null,
        IReadOnlyList<MockStreamResult>? doStream = null)
    {
        _inner = new MockLanguageModelV2(provider, modelId, doGenerate, doStream);
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v3"; }
    }

    /// <summary>Provider id.</summary>
    public string Provider
    {
        get { return _inner.Provider; }
    }

    /// <summary>Model id.</summary>
    public string ModelId
    {
        get { return _inner.ModelId; }
    }

    /// <summary>Generate calls in order.</summary>
    public List<MockLanguageModelCallOptions> DoGenerateCalls
    {
        get { return _inner.DoGenerateCalls; }
    }

    /// <summary>Stream calls in order.</summary>
    public List<MockLanguageModelCallOptions> DoStreamCalls
    {
        get { return _inner.DoStreamCalls; }
    }

    /// <summary>Returns the next scripted generate result.</summary>
    public Task<MockGenerateResult> DoGenerateAsync(MockLanguageModelCallOptions options)
    {
        return _inner.DoGenerateAsync(options);
    }

    /// <summary>Returns the next scripted stream result.</summary>
    public Task<MockStreamResult> DoStreamAsync(MockLanguageModelCallOptions options)
    {
        return _inner.DoStreamAsync(options);
    }
}

/// <summary>Language model v4 test double. Array-backed results are returned in order, starting at the first entry.</summary>
public sealed class MockLanguageModelV4
{
    private readonly MockLanguageModelV2 _inner;

    /// <summary>Creates a model. List results are consumed from the first entry.</summary>
    public MockLanguageModelV4(
        string provider = "mock-provider",
        string modelId = "mock-model-id",
        IReadOnlyList<MockGenerateResult>? doGenerate = null,
        IReadOnlyList<MockStreamResult>? doStream = null)
    {
        _inner = new MockLanguageModelV2(provider, modelId, doGenerate, doStream);
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>Provider id.</summary>
    public string Provider
    {
        get { return _inner.Provider; }
    }

    /// <summary>Model id.</summary>
    public string ModelId
    {
        get { return _inner.ModelId; }
    }

    /// <summary>Generate calls in order.</summary>
    public List<MockLanguageModelCallOptions> DoGenerateCalls
    {
        get { return _inner.DoGenerateCalls; }
    }

    /// <summary>Stream calls in order.</summary>
    public List<MockLanguageModelCallOptions> DoStreamCalls
    {
        get { return _inner.DoStreamCalls; }
    }

    /// <summary>Returns the next scripted generate result.</summary>
    public Task<MockGenerateResult> DoGenerateAsync(MockLanguageModelCallOptions options)
    {
        return _inner.DoGenerateAsync(options);
    }

    /// <summary>Returns the next scripted stream result.</summary>
    public Task<MockStreamResult> DoStreamAsync(MockLanguageModelCallOptions options)
    {
        return _inner.DoStreamAsync(options);
    }
}

/// <summary>Embedding model v3 test double. Array-backed results are returned in order, starting at the first entry.</summary>
public sealed class MockEmbeddingModelV3
{
    private readonly IReadOnlyList<MockEmbedResult>? _results;
    private readonly Func<MockEmbedOptions?, Task<MockEmbedResult>>? _embed;

    /// <summary>Creates a model. List results are consumed from the first entry.</summary>
    public MockEmbeddingModelV3(
        string provider = "mock-provider",
        string modelId = "mock-model-id",
        int? maxEmbeddingsPerCall = 1,
        bool supportsParallelCalls = false,
        IReadOnlyList<MockEmbedResult>? doEmbed = null,
        Func<MockEmbedOptions?, Task<MockEmbedResult>>? embed = null)
    {
        Provider = provider;
        ModelId = modelId;
        MaxEmbeddingsPerCall = maxEmbeddingsPerCall;
        SupportsParallelCalls = supportsParallelCalls;
        _results = doEmbed;
        _embed = embed;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v3"; }
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Maximum embeddings per call.</summary>
    public int? MaxEmbeddingsPerCall { get; }

    /// <summary>Whether calls may run in parallel.</summary>
    public bool SupportsParallelCalls { get; }

    /// <summary>Embed calls in order.</summary>
    public List<MockEmbedOptions> DoEmbedCalls { get; } = new List<MockEmbedOptions>();

    /// <summary>Returns the next scripted embed result.</summary>
    public Task<MockEmbedResult> DoEmbedAsync(MockEmbedOptions? options)
    {
        DoEmbedCalls.Add(options ?? new MockEmbedOptions(null));
        if (_embed != null)
        {
            return _embed(options);
        }

        if (_results != null)
        {
            return Task.FromResult(_results[DoEmbedCalls.Count - 1]);
        }

        throw new NotSupportedException("doEmbed is not implemented.");
    }
}

/// <summary>Embedding model v4 test double. Array-backed results are returned in order, starting at the first entry.</summary>
public sealed class MockEmbeddingModelV4
{
    private readonly MockEmbeddingModelV3 _inner;

    /// <summary>Creates a model. List results are consumed from the first entry.</summary>
    public MockEmbeddingModelV4(
        string provider = "mock-provider",
        string modelId = "mock-model-id",
        int? maxEmbeddingsPerCall = 1,
        int? maxInputBytesPerCall = null,
        bool supportsParallelCalls = false,
        IReadOnlyList<MockEmbedResult>? doEmbed = null)
    {
        _inner = new MockEmbeddingModelV3(provider, modelId, maxEmbeddingsPerCall, supportsParallelCalls, doEmbed);
        MaxInputBytesPerCall = maxInputBytesPerCall;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>Provider id.</summary>
    public string Provider
    {
        get { return _inner.Provider; }
    }

    /// <summary>Model id.</summary>
    public string ModelId
    {
        get { return _inner.ModelId; }
    }

    /// <summary>Maximum embeddings per call.</summary>
    public int? MaxEmbeddingsPerCall
    {
        get { return _inner.MaxEmbeddingsPerCall; }
    }

    /// <summary>Maximum input bytes per call.</summary>
    public int? MaxInputBytesPerCall { get; }

    /// <summary>Whether calls may run in parallel.</summary>
    public bool SupportsParallelCalls
    {
        get { return _inner.SupportsParallelCalls; }
    }

    /// <summary>Embed calls in order.</summary>
    public List<MockEmbedOptions> DoEmbedCalls
    {
        get { return _inner.DoEmbedCalls; }
    }

    /// <summary>Returns the next scripted embed result.</summary>
    public Task<MockEmbedResult> DoEmbedAsync(MockEmbedOptions options)
    {
        return _inner.DoEmbedAsync(options);
    }
}
