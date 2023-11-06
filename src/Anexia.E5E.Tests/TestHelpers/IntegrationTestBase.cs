using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.TestHelpers;

public abstract class IntegrationTestBase : XunitContextBase, IAsyncLifetime
{
	private IHostBuilder _builder;
	protected IHost Host { get; private set; } = null!;

	protected IntegrationTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
	{
		_builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
			.ConfigureLogging(lb => lb.SetMinimumLevel(LogLevel.Debug))
			.ConfigureLogging(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddConsole(options => options.FormatterName = "UnitTest")
					.AddConsoleFormatter<UnitTestConsoleFormatter, UnitTestConsoleFormatterOptions>(cfg =>
					{
						cfg.TestName = XunitContext.Context.MethodName;
					});
			});
	}

	protected virtual IHostBuilder ConfigureE5E(IHostBuilder builder) =>
		builder.UseAnexiaE5E(new TestE5ERuntimeOptions());

	protected virtual IHostBuilder ConfigureHost(IHostBuilder builder) => builder;

	/// <summary>
	/// Called immediately after the class has been created, before it is used.
	/// </summary>
	public Task InitializeAsync()
	{
		_builder = ConfigureE5E(_builder);
		_builder = ConfigureHost(_builder);
		_builder = _builder.ConfigureServices(services =>
			services.AddSingleton<IConsoleAbstraction, TestConsoleAbstraction>());
		this.Host = _builder.Build();

		var sw = Stopwatch.StartNew();
		var lifetime = Host.Services.GetRequiredService<IHostApplicationLifetime>();
		lifetime.ApplicationStarted.Register(() =>
		{
			sw.Stop();
			// Startup shouldn't take longer than three seconds.
			Assert.InRange(sw.ElapsedMilliseconds, 0, 3000);
		}, true);

		return this.Host.StartAsync();
	}

	/// <summary>
	/// Called when an object is no longer needed. Called just before <see cref="M:System.IDisposable.Dispose" />
	/// if the class also implements that.
	/// </summary>
	public Task DisposeAsync() => this.Host.StopAsync(TimeSpan.FromSeconds(1));
}
