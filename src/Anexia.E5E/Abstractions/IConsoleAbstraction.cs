namespace Anexia.E5E.Abstractions;

public interface IConsoleAbstraction : IDisposable, IAsyncDisposable
{
	void Open();
	void CloseStdin();

	Task<string?> ReadLineFromStdinAsync(CancellationToken token = default);
	Task WriteToStdoutAsync(string? s, CancellationToken token = default);
	Task WriteToStderrAsync(string? s, CancellationToken token = default);
}

internal class ConsoleAbstraction : IConsoleAbstraction
{
	private StreamReader? _stdin;
	private StreamWriter? _stdout;
	private StreamWriter? _stderr;

	public void Open()
	{
		var stdinStream = Console.OpenStandardInput();
		var stdoutStream = Console.OpenStandardOutput();
		var stderrStream = Console.OpenStandardError();

		_stdin = new StreamReader(stdinStream);
		_stdout = new StreamWriter(stdoutStream);
		_stderr = new StreamWriter(stderrStream);
	}

	public void CloseStdin() => _stdin?.Close();

	public Task<string?> ReadLineFromStdinAsync(CancellationToken token = default)
	{
		if (_stdin is null)
			throw new InvalidOperationException("Use the Open() method on the console abstraction before using it.");

		return _stdin.ReadLineAsync();
	}

	public async Task WriteToStdoutAsync(string? s, CancellationToken token = default)
	{
		if (_stdout is null)
			throw new InvalidOperationException("Use the Open() method on the console abstraction before using it.");

		await _stdout.WriteAsync(s);
		await _stdout.FlushAsync();
	}

	public async Task WriteToStderrAsync(string? s, CancellationToken token = default)
	{
		if (_stderr is null)
			throw new InvalidOperationException("Use the Open() method on the console abstraction before using it.");

		await _stderr.WriteAsync(s);
		await _stderr.FlushAsync();
	}

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		_stdin?.Dispose();
		_stdout?.Dispose();
		_stderr?.Dispose();
	}

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
	/// <returns>A task that represents the asynchronous dispose operation.</returns>
	public async ValueTask DisposeAsync()
	{
		_stdin?.Dispose();
		if (_stdout != null) await _stdout.DisposeAsync();
		if (_stderr != null) await _stderr.DisposeAsync();
	}
}