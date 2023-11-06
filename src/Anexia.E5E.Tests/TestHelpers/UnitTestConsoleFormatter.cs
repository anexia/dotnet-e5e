using System;
using System.IO;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Anexia.E5E.Tests.TestHelpers;

public sealed class UnitTestConsoleFormatter : ConsoleFormatter
{
	private readonly UnitTestConsoleFormatterOptions _opts;

	public UnitTestConsoleFormatter(IOptions<UnitTestConsoleFormatterOptions> opts) : base("UnitTest")
	{
		_opts = opts.Value;
	}

	/// <summary>Writes the log message to the specified TextWriter.</summary>
	/// <remarks>
	/// if the formatter wants to write colors to the console, it can do so by embedding ANSI color codes into the string
	/// </remarks>
	/// <param name="logEntry">The log entry.</param>
	/// <param name="scopeProvider">The provider of scope data.</param>
	/// <param name="textWriter">The string writer embedding ansi code for colors.</param>
	/// <typeparam name="TState">The type of the object to be written.</typeparam>
	public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider,
		TextWriter textWriter)
	{
		textWriter.WriteLine($"{_opts.TestName} ({DateTimeOffset.Now.ToLocalTime():T}): {logEntry.State}");
		if (logEntry.Exception is not null) textWriter.WriteLine($"\t{logEntry.Exception}");
	}
}

public sealed class UnitTestConsoleFormatterOptions : ConsoleFormatterOptions
{
	public string TestName { get; set; } = "";
}
