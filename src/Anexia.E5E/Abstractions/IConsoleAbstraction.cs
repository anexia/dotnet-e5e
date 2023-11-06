using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Abstractions;

/// <summary>
/// Abstraction of the input and output streams for the console.
/// </summary>
/// <remarks>
/// A concrete implementation is already added with the default <see cref="IHostBuilder"/>,
/// there's no need to provide a custom implementation.
/// </remarks>
public interface IConsoleAbstraction : IDisposable, IAsyncDisposable
{
	/// <summary>
	/// Opens the required console streams for reading and writing.
	/// </summary>
	void Open();

	/// <summary>
	/// Closes the console streams.
	/// </summary>
	void Close();

	/// <summary>
	/// Reads a single line from the standard input.
	/// </summary>
	/// <param name="token">Can be used to cancel the operation.</param>
	/// <returns>The line, null if the stream is closed.</returns>
	Task<string?> ReadLineFromStdinAsync(CancellationToken token = default);

	/// <summary>
	/// Write text to standard output.
	/// </summary>
	/// <param name="s">The text to write.</param>
	Task WriteToStdoutAsync(string? s);

	/// <summary>
	/// Write text to standard error.
	/// </summary>
	/// <param name="s">The text to write.</param>
	Task WriteToStderrAsync(string? s);
}
