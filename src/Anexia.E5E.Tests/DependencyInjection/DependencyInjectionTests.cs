using System;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Fixtures;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.DependencyInjection;

public class DependencyInjectionTests
{
	private readonly ITestOutputHelper _outputHelper;

	public DependencyInjectionTests(ITestOutputHelper outputHelper)
	{
		_outputHelper = outputHelper;
	}

	[Fact]
	public void DependencyIsInjected()
	{
		var host = new HostFixture(_outputHelper,
			builder => builder.ConfigureServices(services =>
				services.AddFunctionHandler<WorkingDependencyInjectionTestHandler>()));
		host.Host.RegisterEntrypoint<WorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions
			.DefaultEntrypointName);
		using var scope = host.Host.Services.CreateScope();
		Assert.NotNull(scope.ServiceProvider.GetService<IE5EFunctionHandler>());
	}

	[Fact]
	public void MissingDependencyThrowsError()
	{
		var host = new HostFixture(_outputHelper,
			builder => builder.ConfigureServices(services =>
				services.AddFunctionHandler<NotWorkingDependencyInjectionTestHandler>()));
		host.Host.RegisterEntrypoint<NotWorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions
			.DefaultEntrypointName);
		using var scope = host.Host.Services.CreateScope();
		var exception = Assert.Throws<InvalidOperationException>(scope.ServiceProvider.GetService<IE5EFunctionHandler>);
		Assert.Contains(nameof(IDoesNotExist), exception.Message);
	}

	[Fact]
	public async Task DependencyInjectionEndToEnd()
	{
		var host = new HostFixture(_outputHelper,
			builder => builder.ConfigureServices(services =>
				services.AddFunctionHandler<WorkingDependencyInjectionTestHandler>()));
		host.Host.RegisterEntrypoint<NotWorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions
			.DefaultEntrypointName);
		await host.InitializeAsync();
		await host.WriteToStdinOnceAsync("ping");
		await host.DisposeAsync();
		Assert.Equal("pong---", host.GetStdout());
	}

	[Theory]
	[InlineData(typeof(IE5EFunctionHandler))] // is an interface
	[InlineData(typeof(E5ERequest))] // does not implement the interface
	public void InvalidTypeThrowsError(Type t)
	{
		var serviceCollection = new ServiceCollection();
		Assert.Throws<InvalidOperationException>(() => serviceCollection.AddFunctionHandler(t));
	}

	// ReSharper disable once MemberCanBePrivate.Global // public visibility is required for the assembly loading mechanism
	public class WorkingDependencyInjectionTestHandler : IE5EFunctionHandler
	{
		public WorkingDependencyInjectionTestHandler(ILogger<WorkingDependencyInjectionTestHandler> logger) { }

		public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default) =>
			Task.FromResult(E5EResponse.From("success"));
	}

	public interface IDoesNotExist
	{
	}

	// ReSharper disable once MemberCanBePrivate.Global // public visibility is required for the assembly loading mechanism
	public class NotWorkingDependencyInjectionTestHandler : IE5EFunctionHandler
	{
		public NotWorkingDependencyInjectionTestHandler(IDoesNotExist _) { }

		public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default) =>
			Task.FromResult(E5EResponse.From("success"));
	}
}
