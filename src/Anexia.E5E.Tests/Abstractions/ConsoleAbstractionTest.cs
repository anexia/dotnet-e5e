using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Anexia.E5E.Extensions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Abstractions;

public class ConsoleAbstractionTest
{
	private readonly IHost _host;

	public ConsoleAbstractionTest(ITestOutputHelper outputHelper)
	{
		// set dummy stdin
		Console.SetIn(new StreamReader(new MemoryStream()));
		_host = Host.CreateDefaultBuilder()
			.UseAnexiaE5E(new TestE5ERuntimeOptions())
			.ConfigureLogging(l => l.AddXUnit(outputHelper))
			.Build();
	}

	[Fact]
	public async Task DefaultConsoleIsReactingToShutdownEvents()
	{
		var watch = Stopwatch.StartNew();
		var lifetime = _host.Services.GetService<IHostApplicationLifetime>()!;
		lifetime.ApplicationStopping.Register(() => watch.Start());
		lifetime.ApplicationStopped.Register(() => watch.Stop());

		await _host.StartAsync();
		await _host.StopAsync(TimeSpan.FromSeconds(3));

		Assert.False(watch.IsRunning);
		Assert.InRange(watch.ElapsedMilliseconds, 0, 3000);
	}
}
