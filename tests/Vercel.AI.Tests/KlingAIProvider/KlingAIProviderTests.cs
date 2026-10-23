// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.KlingAIProvider;

/// <summary>Redirect protection of <c>KlingAIProvider.Create</c>. It has no upstream counterpart.</summary>
public sealed class KlingAIProviderTests
{
    [Fact]
    public void DisablesAutoRedirectOnASuppliedHandler()
    {
        var clientHandler = new HttpClientHandler { AllowAutoRedirect = true };
        var socketsHandler = new SocketsHttpHandler { AllowAutoRedirect = true };
        var wrappedHandler = new HttpClientHandler { AllowAutoRedirect = true };

        Vercel.AI.KlingAI.KlingAIProvider.Create(null, clientHandler);
        Vercel.AI.KlingAI.KlingAIProvider.Create(null, socketsHandler);
        Vercel.AI.KlingAI.KlingAIProvider.Create(null, new PassThroughHandler { InnerHandler = new PassThroughHandler { InnerHandler = wrappedHandler } });

        Assert.False(clientHandler.AllowAutoRedirect);
        Assert.False(socketsHandler.AllowAutoRedirect);
        Assert.False(wrappedHandler.AllowAutoRedirect);
    }

    [Fact]
    public void DisablesAutoRedirectByReflectionOnACustomHandler()
    {
        var handler = new RedirectingHandler { AllowAutoRedirect = true };

        Vercel.AI.KlingAI.KlingAIProvider.Create(null, handler);

        Assert.False(handler.AllowAutoRedirect);
    }

    [Fact]
    public void DisablesAutoRedirectOnEveryHandlerOfAChain()
    {
        var inner = new HttpClientHandler { AllowAutoRedirect = true };
        var middle = new RedirectingDelegatingHandler { AllowAutoRedirect = true, InnerHandler = inner };
        var outer = new RedirectingDelegatingHandler { AllowAutoRedirect = true, InnerHandler = middle };

        Vercel.AI.KlingAI.KlingAIProvider.Create(null, outer);

        Assert.False(outer.AllowAutoRedirect);
        Assert.False(middle.AllowAutoRedirect);
        Assert.False(inner.AllowAutoRedirect);
    }

    [Fact]
    public void LeavesAHandlerWithAReadOnlyOptionAlone()
    {
        var inner = new ReadOnlyRedirectHandler();

        Vercel.AI.KlingAI.KlingAIProvider.Create(null, new PassThroughHandler { InnerHandler = inner });

        Assert.True(inner.AllowAutoRedirect);
    }

    [Fact]
    public void DisablesAutoRedirectOnACustomHandlerWithASetOnlyOption()
    {
        var handler = new SetOnlyRedirectHandler();

        Vercel.AI.KlingAI.KlingAIProvider.Create(null, handler);

        Assert.True(handler.AutoRedirectWasDisabled);
    }

    private sealed class SetOnlyRedirectHandler : HttpMessageHandler
    {
        public bool AutoRedirectWasDisabled { get; private set; }

        public bool AllowAutoRedirect
        {
            set => AutoRedirectWasDisabled = !value;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class RedirectingHandler : HttpMessageHandler
    {
        public bool AllowAutoRedirect { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class RedirectingDelegatingHandler : DelegatingHandler
    {
        public bool AllowAutoRedirect { get; set; }
    }

    private sealed class ReadOnlyRedirectHandler : HttpMessageHandler
    {
        public bool AllowAutoRedirect { get; } = true;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class PassThroughHandler : DelegatingHandler
    {
    }
}
