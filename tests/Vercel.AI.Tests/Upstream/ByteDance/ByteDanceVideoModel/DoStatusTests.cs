// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ByteDance;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using ByteDanceVideoModelType = Vercel.AI.ByteDance.ByteDanceVideoModel;

namespace Vercel.AI.Tests.Upstream.ByteDance.ByteDanceVideoModel;

/// <summary>Port of <c>bytedance-video-model.test.ts</c> &gt; <c>ByteDanceVideoModel</c> &gt; <c>doStatus</c>.</summary>
public sealed class DoStatusTests
{
    private const string Prefix = "packages/bytedance/src/bytedance-video-model.test.ts::ByteDanceVideoModel > doStatus::";
    private const string TasksUrl = "https://ark.ap-southeast.bytepluses.com/api/v3/contents/generations/tasks";
    private const string Succeeded = "{\"id\":\"test-task-id-123\",\"model\":\"seedance-1-0-pro-250528\",\"status\":\"succeeded\",\"content\":{\"video_url\":\"https://bytedance.cdn/files/video-output.mp4\",\"last_frame_url\":\"https://bytedance.cdn/files/video-output-last-frame.png\"},\"usage\":{\"completion_tokens\":100}}";

    [Fact]
    [UpstreamTest(Prefix + "should encode the task ID as a single URL path segment", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_the_task_ID_as_a_single_URL_path_segment()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(Succeeded));
        var taskId = "abc/../../internal";

        await CreateModel(handler).DoStatusAsync(Status(taskId), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TasksUrl + "/" + Uri.EscapeDataString(taskId), handler.Calls[0].Uri.AbsoluteUri);
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [UpstreamTest(Prefix + "should preserve the $taskId task ID as a URL path segment", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_the_taskId_task_ID_as_a_URL_path_segment(string taskId, string encodedTaskId)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(Succeeded));

        await CreateModel(handler).DoStatusAsync(Status(taskId), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TasksUrl + "/" + encodedTaskId, handler.Calls[0].Uri.AbsoluteUri);
    }

    private static ByteDanceVideoModelType CreateModel(ParityHandler handler)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-key" };
        return new ByteDanceVideoModelType(ByteDanceProvider.Create(options, handler), "seedance-1-0-pro-250528", null);
    }

    private static VideoModelCall Status(string taskId)
    {
        return new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), new Dictionary<string, string>(), CancellationToken.None, operation: OperationJson.Parse("{\"taskId\":\"" + taskId + "\"}"));
    }
}
