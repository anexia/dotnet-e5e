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
	private readonly IServiceProvider _provider;
	private readonly IConsoleAbstraction _console;
	private readonly E5ERuntimeOptions _options;
	private readonly ILogger<E5ECommunicationService> _logger;
	private readonly IHostApplicationLifetime _lifetime;

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
		// If the cancellation is requested, dispose our console streams and therefore end the processing loop below.
		stoppingToken.Register(_console.Close);
		_console.Open();

		using var _ = _logger.BeginScope(new { _options.Entrypoint });

		// > No further services are started until ExecuteAsync becomes asynchronous, such as by calling await.
		// > Avoid performing long, blocking initialization work in ExecuteAsync.
		//
		// Therefore we run the actual listening task like the example here:
		// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-7.0#queued-background-tasks
		try
		{
			await Task.Yield();
			await ListenForIncomingMessagesAsync(stoppingToken);
		}
		catch (E5EException e)
		{
			_logger.MessageProcessingFailed(e);
		}
		catch (Exception e)
		{
			_logger.UnexpectedRuntimeException(e);
			throw;
		}
	}

	private async Task ListenForIncomingMessagesAsync(CancellationToken stoppingToken)
	{
		_logger.ListeningForIncomingMessages();

		while (!stoppingToken.IsCancellationRequested)
		{
			var line = await _console.ReadLineFromStdinAsync(stoppingToken);
			line = line?.TrimEnd();

			// Stop the execution as soon as possible.
			if (stoppingToken.IsCancellationRequested)
			{
				_logger.CancellationRequested();
				continue;
			}

			// Skip empty lines. They shouldn't occur in production, but can happen during testing.
			// Also, the ReadLineFromStdinAsync returns null if the underlying stream is closed.
			if (string.IsNullOrEmpty(line))
			{
				_logger.EmptyLineReceived();
				continue;
			}

			if (line == "ping" && _options.KeepAlive)
			{
				await _console.WriteToStdoutAsync("pong");
				await _console.WriteToStdoutAsync(_options.DaemonExecutionTerminationSequence);
				await _console.WriteToStderrAsync(_options.DaemonExecutionTerminationSequence);
				_logger.PingReceived();
				continue;
			}


			var request = ParseRequest(line);
			using (_logger.BeginScope(new { Line = line, Request = request }))
			{
				if (request.Event is null || request.Context is null)
					throw new E5ERuntimeException("Apparently the deserialization failed, the given event is null.");

				using var scope = _provider.CreateScope();
				var handler = scope.ServiceProvider.GetRequiredService<IE5EFunctionHandler>();

				await ExecuteFunctionAsync(handler, request, stoppingToken);
			}

			// If we should run it only once, end the application afterwards.
			if (!_options.KeepAlive)
			{
				_logger.StopApplication();
				_lifetime.StopApplication();
				break;
			}


			// ...and notify the engine of the end on stdout and stderr.
			await _console.WriteToStdoutAsync(_options.DaemonExecutionTerminationSequence);
			await _console.WriteToStderrAsync(_options.DaemonExecutionTerminationSequence);
		}

		_logger.MessageProcessingStopped();
	}

	private async Task ExecuteFunctionAsync(IE5EFunctionHandler handler, E5ERequest request,
		CancellationToken stoppingToken)
	{
		E5EResponse? response;
		try
		{
			_logger.ExecuteFunction(handler.GetType(), request);
			response = await handler.HandleAsync(request, stoppingToken);
		}
		catch (Exception e)
		{
			throw new E5EFunctionExecutionFailedException(request, e);
		}

		try
		{
			_logger.ReceivedResponse(response);
			var json = JsonSerializer.Serialize(response, E5EJsonSerializerOptions.Default);

			await _console.WriteToStdoutAsync(_options.StdoutTerminationSequence);
			await _console.WriteToStdoutAsync(json);
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
			request = JsonSerializer.Deserialize<E5ERequest>(line, E5EJsonSerializerOptions.Default);
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
