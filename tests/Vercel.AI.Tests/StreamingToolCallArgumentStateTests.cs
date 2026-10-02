// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class StreamingToolCallArgumentStateTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("  {")]
    [InlineData("[]")]
    [InlineData("\n[")]
    public void ReturnsTrueFor(string value)
    {
        Assert.True(StreamingToolCallArgumentState.StartsWithStructuredValue(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("\"value\"")]
    public void ReturnsFalseFor(string? value)
    {
        Assert.False(StreamingToolCallArgumentState.StartsWithStructuredValue(value));
    }

    [Fact]
    public void TracksAStructuredValueAcrossDeltas()
    {
        var state = new StreamingToolCallArgumentState("  {\"value\":");
        Assert.False(state.HasCompleteStructuredValue);

        state.Append("1}");
        Assert.True(state.HasCompleteStructuredValue);
    }

    [Fact]
    public void TracksNestedObjectsAndArrays()
    {
        var state = new StreamingToolCallArgumentState("[{\"value\":{\"items\":[1,2]}}]");
        Assert.True(state.HasCompleteStructuredValue);
    }

    [Fact]
    public void IgnoresStructuralCharactersInsideStrings()
    {
        var state = new StreamingToolCallArgumentState("{\"value\":\"braces: } ] { [\"}");
        Assert.True(state.HasCompleteStructuredValue);
    }

    [Fact]
    public void HandlesEscapedQuotesAcrossDeltas()
    {
        var state = new StreamingToolCallArgumentState("{\"value\":\"escaped quote: \\\"");
        Assert.False(state.HasCompleteStructuredValue);

        state.Append(" still in string\"}");
        Assert.True(state.HasCompleteStructuredValue);
    }

    [Fact]
    public void CanBeginAfterAnEmptyOrWhitespaceOnlyDelta()
    {
        var state = new StreamingToolCallArgumentState("  ");
        state.Append("[");
        Assert.False(state.HasCompleteStructuredValue);

        state.Append("]");
        Assert.True(state.HasCompleteStructuredValue);
    }

    [Fact]
    public void DoesNotTreatScalarArgumentsAsACompleteStructuredValue()
    {
        var state = new StreamingToolCallArgumentState("12");
        Assert.False(state.HasCompleteStructuredValue);
    }

    [Fact]
    public void DoesNotRecoverMismatchedStructuresAsComplete()
    {
        var state = new StreamingToolCallArgumentState("{\"value\":]");
        state.Append("}");
        Assert.False(state.HasCompleteStructuredValue);
    }
}
