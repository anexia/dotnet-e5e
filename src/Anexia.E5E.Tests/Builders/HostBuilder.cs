using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Builders;

/// <summary>
/// Used for building a <see cref="Microsoft.Extensions.Hosting.IHost"/> that's abstracted for our integration tests.
/// </summary>
public interface IE5EHostBuilder
{
	/// <summary>
	/// Builds the host for usage in tests. Since <see cref="IE5EHost"/> is implementing <seealso cref="IAsyncDisposable"/>,
	/// it can be easily used with the await using pattern.
	/// </summary>
	/// <example>
	/// await using var host = E5EHostBuilder.New()...
	/// </example>
	/// <returns>An implementation of the host.</returns>
	IE5EHost Build();

	/// <summary>
	/// Adds the given lambda as a named function to the host.
	/// </summary>
	/// <param name="name">The name of the function.</param>
	/// <param name="func">The implementation.</param>
	/// <returns>The same instance of the <see cref="IE5EHostBuilder"/>.</returns>
	IE5EHostBuilder WithFunction(string name, Func<E5ERequest, CancellationToken, Task<E5EResponse>> func);

	IE5EHostBuilder WithDefaultHandler<T>(Func<E5ERequest, E5EResponse<T>> func);
	IE5EHostBuilder WithDefaultHandler<T>(Func<E5ERequest, Task<E5EResponse<T>>> func);
}

public interface IE5EHost : IDisposable, IAsyncDisposable
{
	Task StartAsync(CancellationToken token = default);
	Task StopAsync();


	void WriteToStdinOnce(string input);
	void WriteToStdinOnce(E5ERequest request);

	/// <summary>
	/// Reads a response from the stdout.
	/// </summary>
	/// <param name="timeout">The timeout for the action. If not given, a default timeout of three seconds is used.</param>
	/// <returns>The given line.</returns>
	/// <exception cref="TimeoutException">Thrown, if no line within the timespan was logged.</exception>
	Task<E5EResponse<TValue>> ReadResponseFromStdoutAsync<TValue>(TimeSpan? timeout = null);

	/// <summary>
	/// Reads a line from the stdout.
	/// </summary>
	/// <param name="timeout">The timeout for the action. If not given, a default timeout of three seconds is used.</param>
	/// <returns>The given line.</returns>
	/// <exception cref="TimeoutException">Thrown, if no line within the timespan was logged.</exception>
	Task<string> ReadLineFromStdoutAsync(TimeSpan? timeout = null);

	string Stdout();
	string Stderr();
}

public static class E5EHostBuilder
{
	public static IE5EHostBuilder New(ITestOutputHelper outputHelper, E5ERuntimeOptions? options) =>
		new HostBuilder(outputHelper, options);

	public static IE5EHostBuilder New(ITestOutputHelper outputHelper, string entrypoint) =>
		New(outputHelper, new TestE5ERuntimeOptions { Entrypoint = entrypoint });

	public static IE5EHostBuilder New(ITestOutputHelper outputHelper) => New(outputHelper, new TestE5ERuntimeOptions());

	class HostBuilder : IE5EHostBuilder
	{
		private readonly IHostBuilder _hb;

		public HostBuilder(ITestOutputHelper outputHelper, E5ERuntimeOptions? options)
		{
			_hb = new Microsoft.Extensions.Hosting.HostBuilder()
				.ConfigureE5E(options ?? new TestE5ERuntimeOptions())
				.ConfigureHostOptions((_, hostOptions) =>
					hostOptions.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
				.ConfigureServices(services => services.AddSingleton<IConsoleAbstraction, TestConsoleAbstraction>())
				.ConfigureLogging(lb => lb.AddXUnit(outputHelper).AddDebug().SetMinimumLevel(LogLevel.Debug));
		}

		public IE5EHost Build() => new E5EHostWrapper(_hb.Build());

		public IE5EHostBuilder WithFunction(string name, Func<E5ERequest, CancellationToken, Task<E5EResponse>> func)
		{
			_hb.ConfigureServices(svc => svc.AddE5EFunction(name, func));
			return this;
		}

		public IE5EHostBuilder WithDefaultHandler<T>(Func<E5ERequest, E5EResponse<T>> func)
		{
			_hb.ConfigureServices(svc => svc.AddE5EFunction(TestE5ERuntimeOptions.DefaultEntrypointName,
				(request, _) =>
				{
					var resp = func.Invoke(request);
					return Task.FromResult<E5EResponse>(resp);
				}));
			return this;
		}

		public IE5EHostBuilder WithDefaultHandler<T>(Func<E5ERequest, Task<E5EResponse<T>>> func)
		{
			_hb.ConfigureServices(svc => svc.AddE5EFunction(TestE5ERuntimeOptions.DefaultEntrypointName,
				async (request, _) =>
				{
					var resp = await func.Invoke(request);
					return resp;
				}));
			return this;
		}
	}

	private class E5EHostWrapper : IE5EHost
	{
		private readonly IHost _host;

		public E5EHostWrapper(IHost host)
		{
			_host = host;
		}

		public void Dispose() => _host.Dispose();
		public ValueTask DisposeAsync() => new(_host.StopAsync());

		public Task StartAsync(CancellationToken token = default) => _host.StartAsync(token);
		public Task StopAsync() => _host.StopAsync();

		public void WriteToStdinOnce(string input)
		{
			var console = _host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
						  throw new InvalidOperationException("There's no console registered");
			console.WriteToStdin(input);
			console.Close();
		}

		public void WriteToStdinOnce(E5ERequest request)
		{
			var console = _host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
						  throw new InvalidOperationException("There's no console registered");
			var req = new E5EIncomingRequest
			{
				Context = new E5EContext("test", DateTimeOffset.Now, true),
				Event = request,
			};
			var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);

			console.WriteToStdin(json);
			console.Close();
		}

		public async Task<E5EResponse<TValue>> ReadResponseFromStdoutAsync<TValue>(TimeSpan? timeout)
		{
			timeout ??= TimeSpan.FromSeconds(3);
			using var cts = new CancellationTokenSource(timeout.Value);

			var console = _host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
						  throw new InvalidOperationException("There's no console registered");
			var options = _host.Services.GetRequiredService<E5ERuntimeOptions>();

			string? line = await console.ReadLineFromStdoutAsync(cts.Token);
			while (line != options.StdoutTerminationSequence)
			{
				line = await console.ReadLineFromStdoutAsync(cts.Token);
			}

			// The line is now the termination sequence, let's read the next line.
			line = await console.ReadLineFromStdoutAsync(cts.Token) ??
				   throw new InvalidOperationException("No next line found after termination sequence");

			var resp = JsonSerializer.Deserialize<E5EResponse<TValue>>(line, E5EJsonSerializerOptions.Default);

			// ReSharper disable once NullableWarningSuppressionIsUsed
			return resp!;
		}

		/// <summary>
		/// Reads a line from the stdout.
		/// </summary>
		/// <param name="timeout">The timeout for the action. If not given, a default timeout of three seconds is used.</param>
		/// <returns>The given line.</returns>
		/// <exception cref="TimeoutException">Thrown, if no line within the timespan was logged.</exception>
		public async Task<string> ReadLineFromStdoutAsync(TimeSpan? timeout)
		{
			var console = _host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
						  throw new InvalidOperationException("There's no console registered");

			timeout ??= TimeSpan.FromSeconds(3);
			using var cts = new CancellationTokenSource(timeout.Value);

			var line = await console.ReadLineFromStdoutAsync(cts.Token);
			return line ?? throw new TimeoutException("No valid line was read within the timeout.");
		}

		public string Stdout()
		{
			var console = _host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
						  throw new InvalidOperationException("There's no console registered");
			return console.Stdout();
		}

		public string Stderr()
		{
			var console = _host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
						  throw new InvalidOperationException("There's no console registered");
			return console.Stderr();
		}
	}
}
