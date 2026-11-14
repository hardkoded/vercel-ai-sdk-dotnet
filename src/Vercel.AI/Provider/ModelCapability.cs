// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// Support declared by a provider's model catalog. Upstream <c>ModelCapability</c>
/// (<c>boolean | 'unknown' | { condition: string }</c>). Unknown is distinct from unsupported.
/// A condition describes when support is available; it is documentation, not an expression to execute.
/// </summary>
public sealed class ModelCapability
{
    /// <summary>Supported.</summary>
    public static readonly ModelCapability Supported = new(true, null, false);

    /// <summary>Not supported.</summary>
    public static readonly ModelCapability Unsupported = new(false, null, false);

    /// <summary>Support is not declared. Upstream <c>'unknown'</c>.</summary>
    public static readonly ModelCapability Unknown = new(false, null, true);

    private ModelCapability(bool supported, string? condition, bool isUnknown)
    {
        IsSupported = supported;
        Condition = condition;
        IsUnknown = isUnknown;
    }

    /// <summary>True when support is declared as <c>true</c>.</summary>
    public bool IsSupported { get; }

    /// <summary>True when support is declared as <c>'unknown'</c>.</summary>
    public bool IsUnknown { get; }

    /// <summary>When support is available, for the <c>{ condition }</c> form. Null otherwise.</summary>
    public string? Condition { get; }

    /// <summary>Maps a boolean to <see cref="Supported"/> or <see cref="Unsupported"/>.</summary>
    public static ModelCapability From(bool supported) => supported ? Supported : Unsupported;

    /// <summary>The <c>{ condition }</c> form.</summary>
    public static ModelCapability When(string condition) => new(false, condition, false);
}

/// <summary>A provider option, declared once at the provider level. Upstream <c>ProviderOptionDefinition</c>.</summary>
public sealed class ProviderOptionDefinition
{
    /// <summary>Creates a definition.</summary>
    public ProviderOptionDefinition(string description)
    {
        Description = description;
    }

    /// <summary>What the option does.</summary>
    public string Description { get; }
}
