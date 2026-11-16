// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Examples.Lib;
using Editor = Examples.Lib.ApplyPatchFileEditor;

namespace Vercel.AI.Examples.Tests.ApplyPatchFileEditor;

// examples/ai-functions/src/lib/apply-patch-file-editor.test.ts, describe "apply patch file editor"
public sealed class ApplyPatchFileEditorTests : IAsyncLifetime
{
    private string _temporaryRoot = string.Empty;
    private string _workspace = string.Empty;
    private string _sibling = string.Empty;

    public Task InitializeAsync()
    {
        _temporaryRoot = Directory.CreateTempSubdirectory("ai-sdk-patch-test-").FullName;
        _workspace = Path.Combine(_temporaryRoot, "workspace");
        _sibling = Path.Combine(_temporaryRoot, "workspace-other");
        Directory.CreateDirectory(_workspace);
        Directory.CreateDirectory(_sibling);
        File.WriteAllText(Path.Combine(_sibling, "sentinel.md"), "sentinel\n");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.Delete(_temporaryRoot, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreatesUpdatesAndDeletesNestedWorkspaceFiles()
    {
        var execute = Editor.CreateApplyPatchExecutor(_workspace);
        var filePath = "nested/notes.md";

        Assert.Equal(
            ApplyPatchStatus.Completed,
            (await execute(new("create", new CreateFileOperation(filePath, "+first\n")))).Status);
        Assert.Equal(
            ApplyPatchStatus.Completed,
            (await execute(new("update", new UpdateFileOperation(filePath, "@@\n-first\n+second\n")))).Status);
        Assert.Equal("second", File.ReadAllText(Path.Combine(_workspace, filePath)));
        Assert.Equal(
            ApplyPatchStatus.Completed,
            (await execute(new("delete", new DeleteFileOperation(filePath)))).Status);
        Assert.False(File.Exists(Path.Combine(_workspace, filePath)));
    }

    [Fact]
    public async Task AllowsOrdinaryWorkspaceFilenamesBeginningWithTwoDots()
    {
        var result = await Editor.CreateApplyPatchExecutor(_workspace)(
            new("create", new CreateFileOperation("..notes.md", "+notes\n")));

        Assert.Equal(ApplyPatchStatus.Completed, result.Status);
        Assert.Equal("notes", File.ReadAllText(Path.Combine(_workspace, "..notes.md")));
    }

    [Theory]
    [InlineData("create_file")]
    [InlineData("update_file")]
    [InlineData("delete_file")]
    public async Task RejectsSiblingPrefixPathsFor(string type)
    {
        var target = Path.GetRelativePath(_workspace, Path.Combine(_sibling, "sentinel.md"));
        ApplyPatchOperation operation = type switch
        {
            "create_file" => new CreateFileOperation(target, "+changed\n"),
            "update_file" => new UpdateFileOperation(target, "@@\n-sentinel\n+changed\n"),
            _ => new DeleteFileOperation(target),
        };

        var result = await Editor.CreateApplyPatchExecutor(_workspace)(
            new("outside", operation));

        Assert.Equal(ApplyPatchStatus.Failed, result.Status);
        Assert.Equal("sentinel\n", File.ReadAllText(Path.Combine(_sibling, "sentinel.md")));
    }

    [Fact]
    public async Task RejectsAbsolutePathsOutsideTheWorkspace()
    {
        var result = await Editor.CreateApplyPatchExecutor(_workspace)(
            new("outside", new DeleteFileOperation(Path.Combine(_sibling, "sentinel.md"))));

        Assert.Equal(ApplyPatchStatus.Failed, result.Status);
        Assert.Equal("sentinel\n", File.ReadAllText(Path.Combine(_sibling, "sentinel.md")));
    }

    [Fact]
    public async Task RejectsDirectorySymlinksPointingOutsideTheWorkspace()
    {
        Directory.CreateSymbolicLink(Path.Combine(_workspace, "linked"), _sibling);

        var result = await Editor.CreateApplyPatchExecutor(_workspace)(
            new("symlink", new DeleteFileOperation("linked/sentinel.md")));

        Assert.Equal(ApplyPatchStatus.Failed, result.Status);
        Assert.Equal("sentinel\n", File.ReadAllText(Path.Combine(_sibling, "sentinel.md")));
    }
}
