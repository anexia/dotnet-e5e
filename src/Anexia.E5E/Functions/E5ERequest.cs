namespace Anexia.E5E.Functions;

/// <summary>
///     Contains all information about the current function execution.
///     <param name="Event">
///         Contains the user-provided request information, e.g. HTTP headers, the payload and other
///         information.
///     </param>
///     <param name="Context">Contains E5E-provided metadata about the current execution.</param>
/// </summary>
public record E5ERequest(E5EEvent Event, E5ERequestContext Context);
