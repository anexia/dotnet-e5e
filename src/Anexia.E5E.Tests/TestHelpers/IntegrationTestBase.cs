using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.TestHelpers;

public abstract class IntegrationTestBase : XunitContextBase, IAsyncLifetime
{
	protected IntegrationTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
	{
		Host = TestHostBuilder.New(outputHelper);
	}


	protected TestHostBuilder Host { get; }

	public virtual Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	// Stop the host on shutdown, avoid memory leaks.
	public Task DisposeAsync()
	{
		return Host.StopAsync();
	}
}
