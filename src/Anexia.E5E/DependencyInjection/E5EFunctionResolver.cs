using Anexia.E5E.Functions;

namespace Anexia.E5E.DependencyInjection;

/// <summary>
/// Resolves the implementation for the entrypoint that's passed during startup.
/// </summary>
public delegate IE5EFunction E5EFunctionResolver();
