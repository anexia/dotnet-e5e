using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;

using Xunit;

namespace Anexia.E5E.Tests.Abstractions;

public class ConsoleAbstractionTest
{
	[Fact(Timeout = 10000)]
	public async Task DefaultConsoleIsReactingToShutdownEvents()
	{
		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		await using var console = new ConsoleAbstraction();

		var watch = Stopwatch.StartNew();
		console.Open();
		var line = await console.ReadLineFromStdinAsync(cts.Token);
		watch.Stop();
		Assert.InRange(watch.ElapsedMilliseconds, 0, 2000);
		Assert.Null(line);
	}
}
