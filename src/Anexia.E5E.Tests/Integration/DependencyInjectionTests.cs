using System;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Helpers;
using Anexia.E5E.Tests.TestHelpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

public class DependencyInjectionTests : IntegrationTestBase
{
	public DependencyInjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
	{
	}

	[Theory]
	[InlineData(typeof(IE5EFunctionHandler))] // is an interface
	[InlineData(typeof(E5ERequest))] // does not implement the interface
	public async Task InvalidTypeThrowsError(Type t)
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			Host.ConfigureEndpoints(cfg => cfg.RegisterEntrypoint("", t))
				.StartAsync());
	}

	[Fact]
	public async Task DependenciesAreInjected()
	{
		await Host.ConfigureEndpoints(builder =>
		{
			builder.RegisterEntrypoint<WorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions
				.DefaultEntrypointName);
		}).StartAsync();

		using var scope = Host.Inner.Services.CreateScope();
		Assert.NotNull(scope.ServiceProvider.GetService<IE5EFunctionHandler>());
	}

	[Fact]
	public async Task WritesCorrectOutput()
	{
		await Host.ConfigureEndpoints(builder =>
		{
			builder.RegisterEntrypoint<WorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions
				.DefaultEntrypointName);
		}).StartAsync();

		var response = await Host.WriteOnceAsync("ping");
		Assert.Contains("pong", response.Stdout);
	}

	[Fact]
	public async Task WithMissingDependenciesCrashes()
	{
		await Host.ConfigureEndpoints(builder =>
		{
			builder.RegisterEntrypoint<NotWorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions
				.DefaultEntrypointName);
		}).StartAsync();
		await Assert.ThrowsAsync<TaskCanceledException>(() => Host.WriteExampleRequestOnceAsync());
	}

	private class WorkingDependencyInjectionTestHandler : IE5EFunctionHandler
	{
		public WorkingDependencyInjectionTestHandler(ILogger<WorkingDependencyInjectionTestHandler> logger) { }

		public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(E5EResponse.From("success"));
		}
	}

	private interface IDoesNotExist
	{
	}

	private class NotWorkingDependencyInjectionTestHandler : IE5EFunctionHandler
	{
		private readonly IDoesNotExist _unknown;

		public NotWorkingDependencyInjectionTestHandler(IDoesNotExist _)
		{
			_unknown = _;
		}

		public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(E5EResponse.From("success"));
		}
	}
}
