using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;

using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Tests.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
public class TestConsoleAbstraction : IConsoleAbstraction
{
	private readonly ILogger<TestConsoleAbstraction> _logger;

	public TestConsoleAbstraction(ILogger<TestConsoleAbstraction> logger) => _logger = logger;

	private readonly Queue<string> _stderr = new();
	private readonly Queue<string> _stdin = new();
	private readonly Queue<string> _stdout = new();

	private readonly StringBuilder _stderrStr = new();
	private readonly StringBuilder _stdoutStr = new();

	public void Open() => _logger.LogDebug("Console opened for reading");
	public void CloseStdin() => _logger.LogDebug("Stdin closed");

	public string Stdout() => _stdoutStr.ToString();
	public string Stderr() => _stderrStr.ToString();

	public Task<string?> ReadLineFromStdinAsync(CancellationToken token = default)
	{
		string? res = null;
		while (!token.IsCancellationRequested && !_stdin.TryDequeue(out res))
		{
			// wait until we have a result or the task was cancelled
		}

		return Task.FromResult(res);
	}

	public Task WriteToStdoutAsync(string? s, CancellationToken token = default)
	{
		if (s is null)
			throw new ArgumentNullException(nameof(s));

		_logger.LogInformation("Wrote {text} to stdout", s);
		_stdout.Enqueue(s);
		_stdoutStr.Append(s);
		return Task.CompletedTask;
	}

	public Task WriteToStderrAsync(string? s, CancellationToken token = default)
	{
		if (s is null)
			throw new ArgumentNullException(nameof(s));

		_logger.LogError("Wrote {text} to stderr", s);
		_stderr.Enqueue(s);
		_stderrStr.Append(s);
		return Task.CompletedTask;
	}

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		_stdin.Clear();
		_stdout.Clear();
		_stderr.Clear();
	}

	/// <summary>
	///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
	///     asynchronously.
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation.</returns>
	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}

	public void WriteToStdin(string s)
	{
		_logger.LogInformation("Wrote {text} to stdin", s);
		_stdin.Enqueue(s);
	}

	public Task<string?> ReadLineFromStdoutAsync(CancellationToken token = default)
	{
		string? res;
		while (!_stdout.TryDequeue(out res) && !token.IsCancellationRequested)
		{
			// wait until we have a result
		}

		token.ThrowIfCancellationRequested();

		return Task.FromResult(res);
	}
}
