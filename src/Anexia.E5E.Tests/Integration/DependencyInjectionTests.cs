using System;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Helpers;
using Anexia.E5E.Tests.TestHelpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

public class DependencyInjectionTests
{
	[Theory]
	[InlineData(typeof(IE5EFunctionHandler))] // is an interface
	[InlineData(typeof(E5ERequest))] // does not implement the interface
	public void InvalidTypeThrowsError(Type t)
	{
		var serviceCollection = new ServiceCollection();
		Assert.Throws<InvalidOperationException>(() => serviceCollection.AddFunctionHandler(t));
	}

	public class DependencyInjection_DependenciesAreInjected : IntegrationTestBase
	{
		public DependencyInjection_DependenciesAreInjected(ITestOutputHelper outputHelper) : base(outputHelper)
		{
		}

		protected override IHostBuilder ConfigureHost(IHostBuilder builder)
		{
			return builder.ConfigureServices(services =>
				services.AddFunctionHandler<WorkingDependencyInjectionTestHandler>());
		}

		[Fact]
		public void ShouldBeInjected()
		{
			Host.RegisterEntrypoint<WorkingDependencyInjectionTestHandler>(TestE5ERuntimeOptions.DefaultEntrypointName);
			using var scope = Host.Services.CreateScope();
			Assert.NotNull(scope.ServiceProvider.GetService<IE5EFunctionHandler>());
		}

		[Fact]
		public async Task WritesCorrectOutput()
		{
			await Host.WriteToStdinOnceAsync("ping");
			Assert.Contains("pong", await Host.GetStdoutAsync());
		}
	}

	public class DependencyInjection_MissingDependency : IntegrationTestBase
	{
		public DependencyInjection_MissingDependency(ITestOutputHelper outputHelper) : base(outputHelper)
		{
		}

		protected override IHostBuilder ConfigureHost(IHostBuilder builder)
		{
			return builder.ConfigureServices(services =>
				services.AddFunctionHandler<NotWorkingDependencyInjectionTestHandler>());
		}

		[Fact]
		public void ShouldThrowError()
		{
			using var scope = Host.Services.CreateScope();
			var exception =
				Assert.Throws<InvalidOperationException>(scope.ServiceProvider.GetService<IE5EFunctionHandler>);
			Assert.Contains(nameof(IDoesNotExist), exception.Message);
		}
	}

	// ReSharper disable once MemberCanBePrivate.Global // public visibility is required for the assembly loading mechanism
	public class WorkingDependencyInjectionTestHandler : IE5EFunctionHandler
	{
		public WorkingDependencyInjectionTestHandler(ILogger<WorkingDependencyInjectionTestHandler> logger) { }

		public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(E5EResponse.From("success"));
		}
	}

	public interface IDoesNotExist
	{
	}

	// ReSharper disable once MemberCanBePrivate.Global // public visibility is required for the assembly loading mechanism
	public class NotWorkingDependencyInjectionTestHandler : IE5EFunctionHandler
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
