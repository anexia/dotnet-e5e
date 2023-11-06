using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;

using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Tests.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConsoleAbstraction : IConsoleAbstraction
{
	private readonly ILogger<TestConsoleAbstraction> _logger;

	private readonly TaskCompletionSource<string> _stderrCompletion = new();
	private readonly TaskCompletionSource<string> _stdoutCompletion = new();
	private readonly TaskCompletionSource<string> _wroteFirstLine = new();

	private readonly Queue<string> _stderr = new();
	private readonly Queue<string> _stdin = new();
	private readonly Queue<string> _stdout = new();

	private readonly StringBuilder _stderrStr = new();
	private readonly StringBuilder _stdoutStr = new();

	public TestConsoleAbstraction(ILogger<TestConsoleAbstraction> logger)
	{
		_logger = logger;
	}


	public void Open()
	{
		_logger.LogDebug("Console opened for reading");
	}

	public void Close()
	{
		_logger.LogDebug("Closing console");

		_stdoutCompletion.TrySetResult(_stdoutStr.ToString());
		_stderrCompletion.TrySetResult(_stderrStr.ToString());

		_logger.LogDebug("Closed console");
	}

	public Task<string> GetStdoutAsync() => _stdoutCompletion.Task;
	public Task<string> GetStderrAsync() => _stderrCompletion.Task;


	public async Task<string?> ReadLineFromStdinAsync(CancellationToken token = default)
	{
		await Task.Yield();
		string? res = null;
		while (!token.IsCancellationRequested && !_stdin.TryDequeue(out res))
		{
			// wait until we have a result or the task was cancelled
		}

		return res;
	}

	public Task WriteToStdoutAsync(string? s)
	{
		if (s is null)
			throw new ArgumentNullException(nameof(s));

		_logger.LogDebug("Wrote {text} to stdout", s);
		_stdout.Enqueue(s);
		_stdoutStr.Append(s);

		_wroteFirstLine.TrySetResult(s);

		return Task.CompletedTask;
	}

	public Task WriteToStderrAsync(string? s)
	{
		if (s is null)
			throw new ArgumentNullException(nameof(s));

		_logger.LogDebug("Wrote {text} to stderr", s);
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
		_stdoutCompletion.TrySetCanceled();
		_stderrCompletion.TrySetCanceled();
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
		_stdin.Enqueue(s);
		_logger.LogDebug("Wrote {text} to stdin", s);
	}

	public Task WaitForFirstWriteAsync() => _wroteFirstLine.Task;
}
