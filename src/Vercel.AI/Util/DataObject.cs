// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// Object with an explicit constructor token and own properties.
/// Used where JavaScript distinguishes prototypes and inherited names.
/// </summary>
public sealed class DataObject
{
    /// <summary>Creates an object whose constructor token is <see cref="object"/>.</summary>
    public DataObject()
    {
        Constructor = typeof(object);
    }

    /// <summary>Identity compared by <see cref="DeepEqual.IsDeepEqualData"/> before properties.</summary>
    public object Constructor { get; set; }

    /// <summary>Own properties. Inherited names are not stored here.</summary>
    public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
