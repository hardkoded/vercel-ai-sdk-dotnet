// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class StreamingToolCallTrackerTests
{
    [Fact]
    public void ShouldGenerateAnIdWhenIdIsMissing()
    {
        var tracker = new StreamingToolCallTracker(() => "generated-id");
        tracker.ProcessDelta(Delta(0, null, "fn", "{}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "generated-id", "fn", "{}");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("undefined")]
    public void ShouldThrowWhenFunctionNameIsMissing(string absentName)
    {
        var tracker = new StreamingToolCallTracker();
        var error = Assert.Throws<AiSdkException>(() => tracker.ProcessDelta(Delta(0, "call_1", absentName == "present" ? "fn" : null, null)));
        Assert.Equal("Expected 'function.name' to be a string.", error.Message);
    }

    [Fact]
    public void ShouldThrowWhenFunctionNameIsMissingFromANewCall()
    {
        var tracker = new StreamingToolCallTracker();
        var error = Assert.Throws<AiSdkException>(() => tracker.ProcessDelta(Delta(0, "call_1", null, null)));
        Assert.Equal("Expected 'function.name' to be a string.", error.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldIgnoreABlankFunctionNameWithoutPreventingPriorCallsFromFinalizing(string name)
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "valid_tool", "{\"value\":1}"));
        tracker.ProcessDelta(Delta(1, "call_2", name, "{\"value\":2}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "valid_tool", "{\"value\":1}");
    }

    [Fact]
    public void ShouldRetainContinuationArgumentsForABlankNameWithAMatchingId()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"pa"));
        tracker.ProcessDelta(Delta(null, "call_1", "", "th\":\"a\"}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "read_file", "{\"path\":\"a\"}");
    }

    [Fact]
    public void ShouldRetainContinuationArgumentsForAWhitespaceOnlyNameWithAMatchingIndex()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"pa"));
        tracker.ProcessDelta(Delta(0, null, "   ", "th\":\"a\"}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "read_file", "{\"path\":\"a\"}");
    }

    [Fact]
    public void ShouldKeepIdLessCallsDistinctWhenAnIndexIsReusedAndTypeIsOmitted()
    {
        var tracker = new StreamingToolCallTracker(Sequence("generated-1", "generated-2", "generated-3"));
        tracker.ProcessDelta(Delta(0, null, "read_file", "{\"path\":\"p0\"}"));
        tracker.ProcessDelta(Delta(0, null, "write_file", "{\"path\":\"p1\"}"));
        tracker.ProcessDelta(Delta(0, null, "read_file", "{\"path\":\"p2\"}"));
        var calls = tracker.Flush();
        Assert.Equal(3, calls.Count);
        AssertCall(calls[0], "generated-1", "read_file", "{\"path\":\"p0\"}");
        AssertCall(calls[1], "generated-2", "write_file", "{\"path\":\"p1\"}");
        AssertCall(calls[2], "generated-3", "read_file", "{\"path\":\"p2\"}");
    }

    [Fact]
    public void ShouldKeepCompleteSameNameCallsDistinctWithMissingIdsAndAReusedIndex()
    {
        AssertSameNameCallsStayDistinct(null, "{\"value\":2}");
    }

    [Fact]
    public void ShouldKeepCompleteSameNameCallsDistinctWithARepeatedIdAndAReusedIndex()
    {
        AssertSameNameCallsStayDistinct("dup", "{\"value\":2}");
    }

    [Fact]
    public void ShouldKeepAPartialSameNameCallDistinctWithMissingIdsAndAReusedIndex()
    {
        AssertSameNameCallsStayDistinct(null, "{\"value\":");
    }

    [Fact]
    public void ShouldKeepAPartialSameNameCallDistinctWithARepeatedIdAndAReusedIndex()
    {
        AssertSameNameCallsStayDistinct("dup", "{\"value\":");
    }

    [Fact]
    public void ShouldKeepInterleavedSameNameCallsWithDistinctIdsAndAReusedIndexSeparate()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "same_tool", "{\"value\":"));
        tracker.ProcessDelta(Delta(0, "call_2", "same_tool", "{\"value\":2}"));
        tracker.ProcessDelta(Delta(0, "call_1", null, "1}"));
        var calls = tracker.Flush();
        Assert.Equal(2, calls.Count);
        AssertCall(calls[0], "call_1", "same_tool", "{\"value\":1}");
        AssertCall(calls[1], "call_2", "same_tool", "{\"value\":2}");
    }

    [Fact]
    public void ShouldIgnoreAnIndexOnlyContinuationAfterTheIndexIsReused()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "first", "{\"value\":1}"));
        tracker.ProcessDelta(Delta(0, "call_2", "second", "{\"value\":2}"));
        tracker.ProcessDelta(Delta(0, null, null, "{\"unattributed\":true}"));
        var calls = tracker.Flush();
        Assert.Equal(2, calls.Count);
        AssertCall(calls[0], "call_1", "first", "{\"value\":1}");
        AssertCall(calls[1], "call_2", "second", "{\"value\":2}");
    }

    [Fact]
    public void ShouldUseTheIndexWhenContinuationIdsAreBlank()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"pa"));
        tracker.ProcessDelta(Delta(0, "   ", null, "th\":\"a\"}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "read_file", "{\"path\":\"a\"}");
    }

    [Fact]
    public void ShouldGenerateUniqueIdsForBlankAndRepeatedIds()
    {
        var tracker = new StreamingToolCallTracker(Sequence("generated-1", "generated-2"));
        tracker.ProcessDelta(Delta(0, "", "read_file", "{}"));
        tracker.ProcessDelta(Delta(1, "dup", "read_file", "{}"));
        tracker.ProcessDelta(Delta(2, "dup", "write_file", "{}"));
        var calls = tracker.Flush();
        Assert.Equal(3, calls.Count);
        AssertCall(calls[0], "generated-1", "read_file", "{}");
        AssertCall(calls[1], "dup", "read_file", "{}");
        AssertCall(calls[2], "generated-2", "write_file", "{}");
    }

    [Fact]
    public void ShouldKeepSameNameCallsWithRepeatedIdsAndDistinctIndicesSeparate()
    {
        var tracker = new StreamingToolCallTracker(() => "generated-id");
        tracker.ProcessDelta(Delta(0, "dup", "same_tool", "{\"value\":0}"));
        tracker.ProcessDelta(Delta(1, "dup", "same_tool", "{\"value\":1}"));
        var calls = tracker.Flush();
        Assert.Equal(2, calls.Count);
        AssertCall(calls[0], "dup", "same_tool", "{\"value\":0}");
        AssertCall(calls[1], "generated-id", "same_tool", "{\"value\":1}");
    }

    [Fact]
    public void ShouldPreserveNonblankIdsAndFunctionNamesExactly()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, " spaced ", " same_tool ", "{\"value\":"));
        tracker.ProcessDelta(Delta(0, " spaced ", null, "0}"));
        tracker.ProcessDelta(Delta(1, "spaced", " same_tool ", "{\"value\":1}"));
        var calls = tracker.Flush();
        Assert.Equal(2, calls.Count);
        AssertCall(calls[0], " spaced ", " same_tool ", "{\"value\":0}");
        AssertCall(calls[1], "spaced", " same_tool ", "{\"value\":1}");
    }

    [Fact]
    public void ShouldCreateBoundedUniqueIdsWhenGenerateIdReturnsDuplicates()
    {
        var generated = 0;
        var tracker = new StreamingToolCallTracker(() =>
        {
            generated++;
            return "generated-id";
        });
        tracker.ProcessDelta(Delta(0, null, "first", "{}"));
        tracker.ProcessDelta(Delta(1, null, "second", "{}"));
        tracker.ProcessDelta(Delta(2, null, "third", "{}"));
        var calls = tracker.Flush();
        Assert.Equal(3, generated);
        Assert.Equal(new[] { "generated-id", "generated-id-1", "generated-id-2" }, Ids(calls));
    }

    [Fact]
    public void ShouldCreateUsableIdsWhenGenerateIdReturnsBlankValues()
    {
        var generated = 0;
        var tracker = new StreamingToolCallTracker(() =>
        {
            generated++;
            return "   ";
        });
        tracker.ProcessDelta(Delta(0, null, "first", "{}"));
        tracker.ProcessDelta(Delta(1, null, "second", "{}"));
        var calls = tracker.Flush();
        Assert.Equal(2, generated);
        Assert.Equal(new[] { "tool-call", "tool-call-1" }, Ids(calls));
    }

    [Fact]
    public void ShouldIgnoreUnattributableDeltasWhenMultipleCallsAreActive()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"path\":\"a\"}"));
        tracker.ProcessDelta(Delta(1, "call_2", "write_file", "{\"path\":\"b\"}"));
        tracker.ProcessDelta(Delta(null, null, null, "{\"unattributed\":true}"));
        var calls = tracker.Flush();
        Assert.Equal(2, calls.Count);
        AssertCall(calls[0], "call_1", "read_file", "{\"path\":\"a\"}");
        AssertCall(calls[1], "call_2", "write_file", "{\"path\":\"b\"}");
    }

    [Fact]
    public void ShouldIgnoreAnAmbiguousContinuationForARepeatedId()
    {
        var tracker = new StreamingToolCallTracker(() => "generated-id");
        tracker.ProcessDelta(Delta(0, "dup", "read_file", "{\"path\":\"a\"}"));
        tracker.ProcessDelta(Delta(1, "dup", "write_file", "{\"path\":\"b\"}"));
        tracker.ProcessDelta(Delta(null, "dup", null, "{\"unattributed\":true}"));
        var calls = tracker.Flush();
        Assert.Equal(2, calls.Count);
        AssertCall(calls[0], "dup", "read_file", "{\"path\":\"a\"}");
        AssertCall(calls[1], "generated-id", "write_file", "{\"path\":\"b\"}");
    }

    [Fact]
    public void ShouldUseAMatchingNameAndIndexForAnIdLessContinuation()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"pa"));
        tracker.ProcessDelta(Delta(0, null, "read_file", "th\":\"a\"}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "read_file", "{\"path\":\"a\"}");
    }

    [Fact]
    public void ShouldUseTheIndexWhenAContinuationHasAnUnexpectedId()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"pa"));
        tracker.ProcessDelta(Delta(0, "unexpected", null, "th\":\"a\"}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "read_file", "{\"path\":\"a\"}");
    }

    [Fact]
    public void ShouldContinueACallWhenItsIdChangesButItsIndexAndNameMatch()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "read_file", "{\"pa"));
        tracker.ProcessDelta(Delta(0, "unexpected", "read_file", "th\":\"a\"}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "read_file", "{\"path\":\"a\"}");
    }

    [Fact]
    public void ShouldContinueACallWhenAllLabelsRepeatAfterAParsableArgumentPrefix()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "calculate", "1"));
        tracker.ProcessDelta(Delta(0, "call_1", "calculate", "2"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "calculate", "12");
    }

    [Fact]
    public void ShouldContinueAStructuredArgumentWhenRepeatedLabelsPrecedeANestedObject()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(0, "call_1", "calculate", "{\"value\":"));
        tracker.ProcessDelta(Delta(0, "call_1", "calculate", "{\"nested\":true}}"));
        var call = Assert.Single(tracker.Flush());
        AssertCall(call, "call_1", "calculate", "{\"value\":{\"nested\":true}}");
    }

    [Fact]
    public void ShouldEmitToolCallsInIndexOrder()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(1, "call_1", "second", "{}"));
        tracker.ProcessDelta(Delta(0, "call_0", "first", "{}"));
        Assert.Equal(new[] { "first", "second" }, Names(tracker.Flush()));
    }

    [Fact]
    public void ShouldPreserveInsertionOrderWhenCallsMixPresentAndOmittedIndices()
    {
        var tracker = new StreamingToolCallTracker();
        tracker.ProcessDelta(Delta(null, "call_without_index", "first", "{}"));
        tracker.ProcessDelta(Delta(0, "call_with_index", "second", "{}"));
        Assert.Equal(new[] { "first", "second" }, Names(tracker.Flush()));
    }

    private static void AssertSameNameCallsStayDistinct(string? id, string secondArguments)
    {
        var tracker = new StreamingToolCallTracker(() => "generated-id");
        tracker.ProcessDelta(Delta(0, id, "same_tool", "{\"value\":1}"));
        tracker.ProcessDelta(Delta(0, id, "same_tool", secondArguments));
        var calls = tracker.Flush();
        Assert.Equal(new[] { "{\"value\":1}", secondArguments }, Inputs(calls));
        Assert.Equal(2, DistinctIds(calls));
    }

    private static StreamingToolCallDelta Delta(int? index, string? id, string? name, string? arguments)
    {
        return new StreamingToolCallDelta(index, id, name, arguments);
    }

    private static Func<string> Sequence(params string[] ids)
    {
        var queue = new Queue<string>(ids);
        return () => queue.Dequeue();
    }

    private static void AssertCall(ToolCallStreamPart call, string id, string name, string arguments)
    {
        Assert.Equal(id, call.ToolCallId);
        Assert.Equal(name, call.ToolName);
        Assert.Equal(arguments, call.ArgumentsJson);
    }

    private static string[] Inputs(List<ToolCallStreamPart> calls)
    {
        var values = new string[calls.Count];
        for (var i = 0; i < calls.Count; i++)
        {
            values[i] = calls[i].ArgumentsJson;
        }

        return values;
    }

    private static string[] Ids(List<ToolCallStreamPart> calls)
    {
        var values = new string[calls.Count];
        for (var i = 0; i < calls.Count; i++)
        {
            values[i] = calls[i].ToolCallId;
        }

        return values;
    }

    private static string[] Names(List<ToolCallStreamPart> calls)
    {
        var values = new string[calls.Count];
        for (var i = 0; i < calls.Count; i++)
        {
            values[i] = calls[i].ToolName;
        }

        return values;
    }

    private static int DistinctIds(List<ToolCallStreamPart> calls)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var call in calls)
        {
            ids.Add(call.ToolCallId);
        }

        return ids.Count;
    }
}
