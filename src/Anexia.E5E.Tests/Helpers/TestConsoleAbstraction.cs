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
	private readonly Queue<string> _stderr = new();
	private readonly TaskCompletionSource<string> _stderrCompletion = new();
	private readonly StringBuilder _stderrStr = new();
	private readonly Queue<string> _stdin = new();
	private readonly Queue<string> _stdout = new();
	private readonly TaskCompletionSource<string> _stdoutCompletion = new();
	private readonly StringBuilder _stdoutStr = new();
	private readonly TaskCompletionSource<string> _wroteFirstLine = new();
	private bool _isOpen;

	private readonly ReaderWriterLockSlim _stdinLock = new();
	private readonly ReaderWriterLockSlim _stdoutLock = new();
	private readonly ReaderWriterLockSlim _stderrLock = new();


	public TestConsoleAbstraction(ILogger<TestConsoleAbstraction> logger, IHostApplicationLifetime lifetime)
	{
		_logger = logger;

		// Cancel our operations if the application already tried to shutdown, likely due to a crash.
		lifetime.ApplicationStopping.Register(() => _wroteFirstLine.SetCanceled());
	}

	public void Open()
	{
		_logger.LogDebug("Console opened for reading");
		_isOpen = true;
	}

	public void Close()
	{
		if (!_isOpen) throw new InvalidOperationException("Cannot close an already closed console");

		_logger.LogDebug("Closing console");

		_stdoutCompletion.TrySetResult(_stdoutStr.ToString());
		_stderrCompletion.TrySetResult(_stderrStr.ToString());

		_logger.LogDebug("Closed console");
	}

	public async Task<string?> ReadLineFromStdinAsync(CancellationToken token = default)
	{
		if (!_isOpen) throw new InvalidOperationException("Cannot read from a closed console");

		await Task.Yield();
		string? res = null;
		while (!token.IsCancellationRequested)
		{
			_stdinLock.EnterReadLock();
			if (_stdin.TryDequeue(out res))
			{
				_stdinLock.ExitReadLock();
				return res;
			}

			// wait until we have a result or the task was cancelled
			_stdinLock.ExitReadLock();
		}

		return res;
	}

	public Task WriteToStdoutAsync(string? s)
	{
		ArgumentNullException.ThrowIfNull(s);
		if (!_isOpen) throw new InvalidOperationException("Cannot write to a closed console");

		_stdoutLock.EnterWriteLock();
		_logger.LogDebug("Wrote {text} to stdout", s);
		_stdout.Enqueue(s);
		_stdoutStr.Append(s);

		_wroteFirstLine.TrySetResult(s);
		_stdoutLock.ExitWriteLock();

		return Task.CompletedTask;
	}

	public Task WriteToStderrAsync(string? s)
	{
		ArgumentNullException.ThrowIfNull(s);
		if (!_isOpen) throw new InvalidOperationException("Cannot write to a closed console");
		_stderrLock.EnterWriteLock();

		_logger.LogDebug("Wrote {text} to stderr", s);
		_stderr.Enqueue(s);
		_stderrStr.Append(s);
		_stderrLock.ExitWriteLock();
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

	public Task<string> GetStdoutAsync()
	{
		return _stdoutCompletion.Task;
	}

	public Task<string> GetStderrAsync()
	{
		return _stderrCompletion.Task;
	}

	public void WriteToStdin(string s)
	{
		if (!_isOpen) throw new InvalidOperationException("Cannot write to a closed console");
		_stdinLock.EnterWriteLock();
		_stdin.Enqueue(s);
		_stdinLock.ExitWriteLock();
		_logger.LogDebug("Wrote {text} to stdin", s);
	}

	public Task WaitForFirstWriteAsync()
	{
		return _wroteFirstLine.Task;
	}
}
