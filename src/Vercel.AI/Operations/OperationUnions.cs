// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>An API or a provider that exposes one. Maps to the upstream <c>Api | ProviderV4</c> unions.</summary>
/// <typeparam name="TApi">The API type.</typeparam>
/// <typeparam name="TProvider">The provider type that exposes the API.</typeparam>
public sealed class ApiOrProvider<TApi, TProvider>
    where TApi : class
    where TProvider : class
{
    private ApiOrProvider(TApi? api, TProvider? provider)
    {
        Api = api;
        Provider = provider;
    }

    /// <summary>The API, when this wraps one.</summary>
    public TApi? Api { get; }

    /// <summary>The provider, when this wraps one.</summary>
    public TProvider? Provider { get; }

    /// <summary>Wraps an API.</summary>
    public static ApiOrProvider<TApi, TProvider> FromApi(TApi api)
    {
        return new ApiOrProvider<TApi, TProvider>(api ?? throw new ArgumentNullException(nameof(api)), null);
    }

    /// <summary>Wraps a provider.</summary>
    public static ApiOrProvider<TApi, TProvider> FromProvider(TProvider provider)
    {
        return new ApiOrProvider<TApi, TProvider>(null, provider ?? throw new ArgumentNullException(nameof(provider)));
    }
}

/// <summary>A single item for <c>embed</c> or a list for <c>embedMany</c>, as carried by embed events.</summary>
/// <typeparam name="TOne">The single item type.</typeparam>
/// <typeparam name="TMany">The list type.</typeparam>
public sealed class EmbedEventValue<TOne, TMany>
{
    private EmbedEventValue(bool isMany, TOne? one, TMany? many)
    {
        IsMany = isMany;
        One = one;
        Many = many;
    }

    /// <summary>True for <c>embedMany</c>.</summary>
    public bool IsMany { get; }

    /// <summary>The single item. Default when <see cref="IsMany"/> is true.</summary>
    public TOne? One { get; }

    /// <summary>The list. Default when <see cref="IsMany"/> is false.</summary>
    public TMany? Many { get; }

    /// <summary>Wraps the single <c>embed</c> item.</summary>
    public static EmbedEventValue<TOne, TMany> FromOne(TOne value)
    {
        return new EmbedEventValue<TOne, TMany>(false, value, default);
    }

    /// <summary>Wraps the <c>embedMany</c> list.</summary>
    public static EmbedEventValue<TOne, TMany> FromMany(TMany values)
    {
        return new EmbedEventValue<TOne, TMany>(true, default, values);
    }
}
