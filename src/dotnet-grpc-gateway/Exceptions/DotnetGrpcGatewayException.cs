#nullable enable
using System;

namespace DotNetGrpcGateway.Exceptions;

/// <summary>
/// Base exception type for all custom exceptions in the DotNetGrpcGateway project.
/// </summary>
public class DotnetGrpcGatewayException : Exception
{
    public DotnetGrpcGatewayException() { }

    public DotnetGrpcGatewayException(string message) : base(message) { }

    public DotnetGrpcGatewayException(string message, Exception innerException) : base(message, innerException) { }
}
