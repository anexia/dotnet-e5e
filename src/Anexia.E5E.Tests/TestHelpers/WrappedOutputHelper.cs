using Xunit.Abstractions;

namespace Anexia.E5E.Tests.TestHelpers;

public class WrappedOutputHelper : ITestOutputHelper
{
	private readonly ITestOutputHelper _inner;
	private readonly string _prefix;

	public WrappedOutputHelper(ITestOutputHelper inner, string prefix)
	{
		_inner = inner;
		_prefix = $"[{prefix}]";
	}

	/// <summary>Adds a line of text to the output.</summary>
	/// <param name="message">The message</param>
	public void WriteLine(string message)
	{
		_inner.WriteLine(_prefix + message);
	}

	/// <summary>Formats a line of text and adds it to the output.</summary>
	/// <param name="format">The message format</param>
	/// <param name="args">The format arguments</param>
	public void WriteLine(string format, params object[] args)
	{
		_inner.WriteLine(_prefix + format, args);
	}
}
