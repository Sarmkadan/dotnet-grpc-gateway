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

/// <summary>
/// Thrown when an entity is not found in the repository
/// </summary>
public class NotFoundException : GatewayException
{
    public NotFoundException(string entityType, object id)
        : base($"{entityType} with ID {id} not found", "ENTITY_NOT_FOUND", 404)
    {
        AddDetail("entity_type", entityType);
        AddDetail("entity_id", id);
    }

    public NotFoundException(string entityType, string identifier)
        : base($"{entityType} with identifier '{identifier}' not found", "ENTITY_NOT_FOUND", 404)
    {
        AddDetail("entity_type", entityType);
        AddDetail("identifier", identifier);
    }
}
