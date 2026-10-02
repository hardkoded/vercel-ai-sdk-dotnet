// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// Stands in for a JavaScript <c>undefined</c> value inside dictionaries and arrays.
/// A C# <c>null</c> maps to JSON <c>null</c>.
/// </summary>
public sealed class JsonUndefined
{
    private JsonUndefined()
    {
    }

    /// <summary>The only undefined sentinel.</summary>
    public static JsonUndefined Value { get; } = new JsonUndefined();
}
