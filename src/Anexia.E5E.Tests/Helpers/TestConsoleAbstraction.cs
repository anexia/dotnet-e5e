using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Tests.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConsoleAbstraction : IConsoleAbstraction
{
	private readonly ILogger<TestConsoleAbstraction> _logger;
	private readonly CancellationTokenSource _closedCts;
	private readonly CancellationTokenSource _wroteFirstTimeCts = new();

	private readonly Queue<string> _stderr = new();
	private readonly Queue<string> _stdin = new();
	private readonly Queue<string> _stdout = new();

	private readonly StringBuilder _stderrStr = new();
	private readonly StringBuilder _stdoutStr = new();

	public TestConsoleAbstraction(ILogger<TestConsoleAbstraction> logger)
	{
		_logger = logger;
		_closedCts = new CancellationTokenSource();
	}


	public void Open()
	{
		_logger.LogDebug("Console opened for reading");
	}

	public void Close()
	{
		if (_closedCts.IsCancellationRequested)
			throw new InvalidOperationException("The console is already closed, cannot close it again.");

		_logger.LogDebug("Closing console");
		_closedCts.Cancel();
		_logger.LogDebug("Closed console");
	}

	public string Stdout()
	{
		WaitUntilClosed();
		return _stdoutStr.ToString();
	}

	public string Stderr()
	{
		WaitUntilClosed();
		return _stderrStr.ToString();
	}

	public Task<string?> ReadLineFromStdinAsync(CancellationToken token = default)
	{
		string? res = null;
		while (!token.IsCancellationRequested && !_stdin.TryDequeue(out res))
		{
			// wait until we have a result or the task was cancelled
		}

		return Task.FromResult(res);
	}

	public Task WriteToStdoutAsync(string? s)
	{
		if (s is null)
			throw new ArgumentNullException(nameof(s));

		_logger.LogDebug("Wrote {text} to stdout", s);
		_stdout.Enqueue(s);
		_stdoutStr.Append(s);
		_wroteFirstTimeCts.Cancel();

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
		_closedCts.Dispose();
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
		_stdin.Enqueue(s);
		_logger.LogDebug("Wrote {text} to stdin", s);
	}

	public string ReadLineFromStdout()
	{
		WaitUntilClosed();
		var hasLine = _stdout.TryDequeue(out var line);
		if (!hasLine || line is null)
			throw new InvalidOperationException("Cannot read more lines from stdout");

		return line;
	}

	private void WaitUntilClosed(int timeoutMs = DefaultTimeoutMs)
	{
		_closedCts.Token.WaitHandle.WaitOne(timeoutMs);
	}

	private const int DefaultTimeoutMs = 3000;

	/// <summary>
	/// Waits until the application wrote to one of the streams.
	///
	/// This is a workaround only required for tests, because otherwise the <see cref="IHost"/> might be closed
	/// before the first line was even read.
	/// </summary>
	public void WaitForFirstWriteAction()
	{
		_logger.LogDebug("Waiting for first write action...");
		_wroteFirstTimeCts.Token.WaitHandle.WaitOne(DefaultTimeoutMs);
		_logger.LogDebug("Line to _stdout written: {Line}", _stdout.Peek());
	}
}
