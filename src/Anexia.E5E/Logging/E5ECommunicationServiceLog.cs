using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;

using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Logging;

internal static partial class E5ECommunicationServiceLog
{
	[LoggerMessage(
		Level = LogLevel.Information,
		EventId = 1000,
		Message = "Waiting for E5E requests on standard input")]
	public static partial void ListeningForIncomingMessages(this ILogger<E5ECommunicationService> logger);

	[LoggerMessage(
		Level = LogLevel.Information,
		EventId = 1001,
		Message = "Message processing stopped successfully")]
	public static partial void MessageProcessingStopped(this ILogger<E5ECommunicationService> logger);

	[LoggerMessage(
		Level = LogLevel.Information,
		EventId = 1002,
		Message = "Cancellation requested, processing stopped")]
	public static partial void CancellationRequested(this ILogger<E5ECommunicationService> logger);

	[LoggerMessage(
		Level = LogLevel.Critical,
		EventId = 1010,
		Message = "The communication service crashed unexpectedly")]
	public static partial void UnexpectedRuntimeException(this ILogger<E5ECommunicationService> logger,
		Exception reason);

	[LoggerMessage(
		Level = LogLevel.Debug,
		EventId = 2000,
		Message = "Deserializing {Line}"
	)]
	public static partial void DeserializingLine(this ILogger<E5ECommunicationService> logger, string line);

	[LoggerMessage(
		Level = LogLevel.Debug,
		EventId = 3000,
		Message = "Executing {FunctionHandlerType} with {Request}"
	)]
	public static partial void ExecuteFunction(this ILogger<E5ECommunicationService> logger, Type functionHandlerType,
		E5ERequest request);

	[LoggerMessage(
		Level = LogLevel.Debug,
		EventId = 3001,
		Message = "Received an empty line, ignored it"
	)]
	public static partial void EmptyLineReceived(this ILogger<E5ECommunicationService> logger);

	[LoggerMessage(
		Level = LogLevel.Debug,
		EventId = 3002,
		Message = "Received {Response} from handler"
	)]
	public static partial void ReceivedResponse(this ILogger<E5ECommunicationService> logger, E5EResponse response);

	[LoggerMessage(
		Level = LogLevel.Debug,
		EventId = 4000,
		Message = "Responded to ping message from E5E"
	)]
	public static partial void PingReceived(this ILogger<E5ECommunicationService> logger);

	[LoggerMessage(
		Level = LogLevel.Information,
		EventId = 4001,
		Message = $"Stopping application, because {nameof(E5ERuntimeOptions.KeepAlive)} is set to false"
	)]
	public static partial void RequestStopDueToKeepAliveFalse(this ILogger<E5ECommunicationService> logger);
}
