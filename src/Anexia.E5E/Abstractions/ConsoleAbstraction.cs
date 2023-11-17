using Anexia.E5E.Extensions;

namespace Anexia.E5E.Abstractions;

/// <summary>
///     The default console abstraction for the E5E library. Writes to <see cref="Console.Out" /> and
///     <see cref="Console.Error" />,
///     reads from <see cref="Console.In" />.
/// </summary>
public sealed class ConsoleAbstraction : IConsoleAbstraction
{
	private StreamWriter? _stderr;
	private StreamReader? _stdin;
	private StreamWriter? _stdout;

	/// <summary>
	///     Opens the console streams for reading and writing.
	/// </summary>
	public void Open()
	{
		var stdinStream = Console.OpenStandardInput();
		var stdoutStream = Console.OpenStandardOutput();
		var stderrStream = Console.OpenStandardError();

		_stdin = new StreamReader(stdinStream);
		_stdout = new StreamWriter(stdoutStream);
		_stderr = new StreamWriter(stderrStream);
	}

	/// <summary>
	///     Closes all streams.
	/// </summary>
	public void Close()
	{
		Dispose();
	}

	/// <summary>
	///     Reads one line from the standard input, that is <see cref="Console.In" />. If the method is cancelled with the
	///     given <see cref="CancellationToken" />, it returns <code>null</code>.
	/// </summary>
	/// <param name="token">Token to cancel the operation.</param>
	/// <returns>The line if any, null if the underlying stream is closed.</returns>
	/// <exception cref="InvalidOperationException">Thrown if <see cref="Open" /> was not called beforehand.</exception>
	public async Task<string?> ReadLineFromStdinAsync(CancellationToken token = default)
	{
		if (_stdin is null)
			throw new InvalidOperationException("Use the Open() method on the console abstraction before using it.");

		string? line;
		try
		{
			line = await _stdin.ReadLineAsync().WithWaitCancellation(token).ConfigureAwait(false);
		}
		catch (TaskCanceledException)
		{
			return null;
		}

		return line;
	}

	/// <summary>
	///     Writes a given string to <see cref="Console.Out" /> immediately.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <exception cref="InvalidOperationException">Thrown if <see cref="Open" /> was not called beforehand.</exception>
	public async Task WriteToStdoutAsync(string? s)
	{
		if (_stdout is null)
			throw new InvalidOperationException("Use the Open() method on the console abstraction before using it.");

		await _stdout.WriteAsync(s).ConfigureAwait(false);
		await _stdout.FlushAsync().ConfigureAwait(false);
	}

	/// <summary>
	///     Writes a given string to <see cref="Console.Error" /> immediately.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <exception cref="InvalidOperationException">Thrown if <see cref="Open" /> was not called beforehand.</exception>
	public async Task WriteToStderrAsync(string? s)
	{
		if (_stderr is null)
			throw new InvalidOperationException("Use the Open() method on the console abstraction before using it.");

		await _stderr.WriteAsync(s).ConfigureAwait(false);
		await _stderr.FlushAsync().ConfigureAwait(false);
	}

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		_stdin?.Dispose();
		_stdout?.Dispose();
		_stderr?.Dispose();
	}

	/// <summary>
	///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
	///     asynchronously.
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation.</returns>
	public async ValueTask DisposeAsync()
	{
		_stdin?.Dispose();
		if (_stdout != null) await _stdout.DisposeAsync().ConfigureAwait(false);
		if (_stderr != null) await _stderr.DisposeAsync().ConfigureAwait(false);
	}
}
