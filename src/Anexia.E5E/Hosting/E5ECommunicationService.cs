using System.Text;
using System.Text.Json;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Logging;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Hosting;

internal sealed class E5ECommunicationService : BackgroundService
{
	private readonly IConsoleAbstraction _console;
	private readonly IHostApplicationLifetime _lifetime;
	private readonly ILogger<E5ECommunicationService> _logger;
	private readonly E5ERuntimeOptions _options;
	private readonly IServiceProvider _provider;

	public E5ECommunicationService(
		IServiceProvider provider,
		IConsoleAbstraction console,
		E5ERuntimeOptions options,
		ILogger<E5ECommunicationService> logger,
		IHostApplicationLifetime lifetime)
	{
		_provider = provider;
		_console = console;
		_options = options;
		_logger = logger;
		_lifetime = lifetime;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var _ = _logger.BeginScope(new { _options.Entrypoint });

		// > No further services are started until ExecuteAsync becomes asynchronous, such as by calling await.
		// > Avoid performing long, blocking initialization work in ExecuteAsync.
		//
		// Therefore we run the actual listening task like the example here:
		// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-7.0#queued-background-tasks
		try
		{
			_console.Open();
			await Task.Yield();
			await ListenForIncomingMessagesAsync(stoppingToken).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_logger.UnexpectedRuntimeException(e);
			Environment.ExitCode = 200;
			throw;
		}
		finally
		{
			// Once the message listener returned, close our console.
			_console.Close();

			// And stop the whole application if necessary.
			if (!_options.KeepAlive)
			{
				_logger.RequestStopDueToKeepAliveFalse();
				_lifetime.StopApplication();
			}
		}
	}

	private async Task ListenForIncomingMessagesAsync(CancellationToken stoppingToken)
	{
		_logger.ListeningForIncomingMessages();
		while (!stoppingToken.IsCancellationRequested)
		{
			var line = await _console.ReadLineFromStdinAsync(stoppingToken).ConfigureAwait(false);
			line = line?.TrimEnd();

			// Skip empty lines. They shouldn't occur in production, but can happen during testing.
			// Also, the ReadLineFromStdinAsync returns null if the underlying stream is closed.
			if (string.IsNullOrEmpty(line))
			{
				_logger.EmptyLineReceived();
				continue;
			}

			var response = await RespondToLineAsync(line, stoppingToken).ConfigureAwait(false);
			await _console.WriteToStdoutAsync(response).ConfigureAwait(false);

			// If we should keep it alive (which is notably the default), do that.
			if (_options.KeepAlive)
			{
				// After every *successful* execution, we need to write the execution sequence. This applies to ping
				// messages as well. We do not write it after errors, otherwise our logs would get sent into the void.
				await _console.WriteToStdoutAsync(_options.DaemonExecutionTerminationSequence).ConfigureAwait(false);
				await _console.WriteToStderrAsync(_options.DaemonExecutionTerminationSequence).ConfigureAwait(false);
				continue;
			}

			// Otherwise, we just stop the processing at this point.
			break;
		}

		_logger.MessageProcessingStopped();
	}

	private Task<string> RespondToLineAsync(string line, CancellationToken stoppingToken)
	{
		if (line == "ping" && _options.KeepAlive)
		{
			_logger.PingReceived();
			return Task.FromResult("pong");
		}

		var request = ParseRequest(line);
		using var _ = _logger.BeginScope(new { Line = line, Request = request });

		if (request.Event is null || request.Context is null)
			throw new E5ERuntimeException("Apparently the deserialization failed, the given event is null.");

		using var scope = _provider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<IE5EFunctionHandler>();

		return ExecuteFunctionAsync(handler, request, stoppingToken);
	}

	private async Task<string> ExecuteFunctionAsync(IE5EFunctionHandler handler, E5ERequest request,
		CancellationToken stoppingToken)
	{
		E5EResponse? response;
		try
		{
			_logger.ExecuteFunction(handler.GetType(), request);
			response = await handler.HandleAsync(request, stoppingToken).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			throw new E5EFunctionExecutionFailedException(request, e);
		}

		try
		{
			_logger.ReceivedResponse(response);
#if NET8_0_OR_GREATER
			var json = JsonSerializer.Serialize(response, E5ESerializationContext.Default.E5EResponse);
#else
			var json = JsonSerializer.Serialize<E5EResponse>(response, E5EJsonSerializerOptions.Default);
#endif

			// Although we could use a dedicated response class for serialization, this would create another overhead
			// which is easily replaced with this string concatenation.
			return new StringBuilder().Append(_options.StdoutTerminationSequence)
				.Append("{\"result\":")
				.Append(json)
				.Append('}')
				.ToString();
		}
		catch (Exception e)
		{
			throw new E5EFailedSerializationException("JSON serialization of the response failed", e);
		}
	}

	private E5ERequest ParseRequest(string line)
	{
		E5ERequest? request;
		try
		{
			_logger.DeserializingLine(line);

#if NET8_0_OR_GREATER
			request = JsonSerializer.Deserialize(line, E5ESerializationContext.Default.E5ERequest);
#else
			request = JsonSerializer.Deserialize<E5ERequest>(line, E5EJsonSerializerOptions.Default);
#endif
		}
		catch (JsonException e)
		{
			throw new E5EFailedDeserializationException(line, e);
		}

		if (request?.Event is null)
		{
			throw new E5ERuntimeException("The request event from the deserialization is null.");
		}

		return request;
	}
}
