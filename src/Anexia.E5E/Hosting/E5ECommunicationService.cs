using System.Text.Json;

using Anexia.E5E.Abstractions;
using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Hosting;

internal class E5ECommunicationService : BackgroundService
{
	private readonly IE5EFunction _function;
	private readonly IConsoleAbstraction _console;
	private readonly E5ERuntimeOptions _options;
	private readonly ILogger<E5ECommunicationService> _logger;
	private readonly IHostApplicationLifetime _lifetime;

	public E5ECommunicationService(
		E5EFunctionResolver resolve,
		IConsoleAbstraction console,
		E5ERuntimeOptions options,
		ILogger<E5ECommunicationService> logger,
		IHostApplicationLifetime lifetime)
	{
		_function = resolve();
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
			_logger.LogError(e, "An error occured during message processing");
		}
		catch (Exception e)
		{
			_logger.LogCritical(e, "The background process crashed unexpectedly");
			throw;
		}
	}

	private async Task ListenForIncomingMessagesAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Execution started, waiting for E5E requests on standard input");

		while (!stoppingToken.IsCancellationRequested)
		{
			var line = await _console.ReadLineFromStdinAsync(stoppingToken);
			line = line?.TrimEnd();

			// Stop the execution as soon as possible.
			if (stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Cancellation requested, processing stopped");
				continue;
			}

			// Skip empty lines. They shouldn't occur in production, but can happen during testing.
			// Also, the ReadLineFromStdinAsync returns null if the underlying stream is closed.
			if (string.IsNullOrEmpty(line))
			{
				_logger.LogDebug("Empty line received, processing skipped");
				continue;
			}

			if (line == "ping" && _options.KeepAlive)
			{
				await _console.WriteToStdoutAsync("pong");
				_logger.LogInformation("Responded to ping message from E5E");
				continue;
			}

			using var _ = _logger.BeginScope("Processing line '{line}'", line);
			try
			{
				await ExecuteFunctionAsync(line, stoppingToken);
			}
			catch (E5EException e)
			{
				// do not abort on our exceptions, only on others, unexpected ones.
				_logger.LogError(e, "The function execution failed");
			}

			// If we should run it only once, end the application afterwards.
			if (!_options.KeepAlive)
			{
				_logger.LogDebug("Stopping application, because KeepAlive is set to false");
				_lifetime.StopApplication();
				break;
			}

			// ...and notify the engine of the end on stdout and stderr.
			await _console.WriteToStdoutAsync(_options.DaemonExecutionTerminationSequence);
			await _console.WriteToStderrAsync(_options.DaemonExecutionTerminationSequence);
		}

		_logger.LogInformation("Execution stopped successfully");
	}

	private async Task ExecuteFunctionAsync(string line, CancellationToken stoppingToken)
	{
		E5EIncomingRequest request = ParseRequest(line);
		if (request.Event is null || request.Context is null)
			throw new E5ERuntimeException(
				"Apparently the deserialization failed, the given event is null. Please create a bug report at https://github.com/anexia/dotnet-e5e/issues/new");

		using var _ = _logger.BeginScope(request);

		E5EResponse? response;
		try
		{
			_logger.LogDebug("Executing function with {Event}", request.Event);
			response = await _function.RunAsync(request.Event, stoppingToken);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "The function execution failed for the given {Event}", request.Event);
			throw new E5EFunctionExecutionFailedException(request.Context, request.Event, e);
		}

		try
		{
			_logger.LogDebug("Received {Response}", response);
			var json = JsonSerializer.Serialize(response, E5EJsonSerializerOptions.Default);

			await _console.WriteToStdoutAsync(_options.StdoutTerminationSequence);
			await _console.WriteToStdoutAsync(json);
		}
		catch (Exception e)
		{
			throw new E5EFailedSerializationException("JSON serialization of the response failed", e);
		}
	}

	private E5EIncomingRequest ParseRequest(string line)
	{
		E5EIncomingRequest? request;
		try
		{
			_logger.LogDebug("Deserializing line");
			request = JsonSerializer.Deserialize<E5EIncomingRequest>(line, E5EJsonSerializerOptions.Default);
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
