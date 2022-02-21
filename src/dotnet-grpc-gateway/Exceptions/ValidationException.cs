#nullable enable
using System;

namespace DotNetGrpcGateway.Exceptions;

/// <summary>
/// Thrown when validation of input data fails.
/// </summary>
public class ValidationException : DotnetGrpcGatewayException
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
