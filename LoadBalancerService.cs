_logger.LogInformation("GetNextEndpoint called with {ServiceId}", serviceId);
        var result = GetNextEndpoint(serviceId);
        _logger.LogInformation("GetNextEndpoint completed with {Result}", result);
        return result;